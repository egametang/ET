using System;
using System.Net;
using System.Net.Sockets;

namespace ET
{
    public class IPEndPointNonAlloc : IPEndPoint
    {
#if UNITY
        
        // Two steps to remove allocations in ReceiveFrom_Internal:
        //
        // 1.) remoteEndPoint.Serialize():
        //     https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L1733
        //     -> creates an EndPoint for ReceiveFrom_Internal to write into
        //     -> it's never read from:
        //        ReceiveFrom_Internal passes it to native:
        //          https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L1885
        //        native recv populates 'sockaddr* from' with the remote address:
        //          https://docs.microsoft.com/en-us/windows/win32/api/winsock/nf-winsock-recvfrom
        //     -> can NOT be null. bricks both Unity and Unity Hub otherwise.
        //     -> it seems as if Serialize() is only called to avoid allocating
        //        a 'new SocketAddress' in ReceiveFrom. it's up to the EndPoint.
        //
        // 2.) EndPoint.Create(SocketAddress):
        //     https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L1761
        //     -> SocketAddress is the remote's address that we want to return
        //     -> to avoid 'new EndPoint(SocketAddress), it seems up to the user
        //        to decide how to create a new EndPoint via .Create
        //     -> SocketAddress is the object that was returned by Serialize()
        //
        // in other words, all we need is an extra SocketAddress field that we
        // can pass to ReceiveFrom_Internal to write the result into.
        // => callers can then get the result from the extra field!
        // => no allocations
        //
        // IMPORTANT: remember that IPEndPointNonAlloc is always the same object
        //            and never changes. only the helper field is changed.
        public SocketAddress temp;

        // constructors simply create the field once by calling the base method.
        // (our overwritten method would create anything new)
        public IPEndPointNonAlloc(long address, int port) : base(address, port)
        {
            temp = base.Serialize();
        }
        public IPEndPointNonAlloc(IPAddress address, int port) : base(address, port)
        {
            temp = base.Serialize();
        }

        // Serialize simply returns it
        public override SocketAddress Serialize() => temp;

        // Create doesn't need to create anything.
        // SocketAddress object is already the one we returned in Serialize().
        // ReceiveFrom_Internal simply wrote into it.
        public override EndPoint Create(SocketAddress socketAddress)
        {
            // original IPEndPoint.Create validates:
            if (socketAddress.Family != AddressFamily)
                throw new ArgumentException($"Unsupported socketAddress.AddressFamily: {socketAddress.Family}. Expected: {AddressFamily}");
            if (socketAddress.Size < 8)
                throw new ArgumentException($"Unsupported socketAddress.Size: {socketAddress.Size}. Expected: <8");

            // double check to guarantee that ReceiveFrom actually did write
            // into our 'temp' field. just in case that's ever changed.
            if (socketAddress != temp)
            {
                // well this is fun.
                // in the latest mono from the above github links,
                // the result of Serialize() is passed as 'ref' so ReceiveFrom
                // does in fact write into it.
                //
                // in Unity 2019 LTS's mono version, it does create a new one
                // each time. this is from ILSpy Receive_From:
                //
                //     SocketPal.CheckDualModeReceiveSupport(this);
                //     ValidateBlockingMode();
                //     if (NetEventSource.IsEnabled)
                //     {
                //         NetEventSource.Info(this, $"SRC{LocalEndPoint} size:{size} remoteEP:{remoteEP}", "ReceiveFrom");
                //     }
                //     EndPoint remoteEP2 = remoteEP;
                //     System.Net.Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref remoteEP2);
                //     System.Net.Internals.SocketAddress socketAddress2 = IPEndPointExtensions.Serialize(remoteEP2);
                //     int bytesTransferred;
                //     SocketError socketError = SocketPal.ReceiveFrom(_handle, buffer, offset, size, socketFlags, socketAddress.Buffer, ref socketAddress.InternalSize, out bytesTransferred);
                //     SocketException ex = null;
                //     if (socketError != 0)
                //     {
                //         ex = new SocketException((int)socketError);
                //         UpdateStatusAfterSocketError(ex);
                //         if (NetEventSource.IsEnabled)
                //         {
                //             NetEventSource.Error(this, ex, "ReceiveFrom");
                //         }
                //         if (ex.SocketErrorCode != SocketError.MessageSize)
                //         {
                //             throw ex;
                //         }
                //     }
                //     if (!socketAddress2.Equals(socketAddress))
                //     {
                //         try
                //         {
                //             remoteEP = remoteEP2.Create(socketAddress);
                //         }
                //         catch
                //         {
                //         }
                //         if (_rightEndPoint == null)
                //         {
                //             _rightEndPoint = remoteEP2;
                //         }
                //     }
                //     if (ex != null)
                //     {
                //         throw ex;
                //     }
                //     if (NetEventSource.IsEnabled)
                //     {
                //         NetEventSource.DumpBuffer(this, buffer, offset, size, "ReceiveFrom");
                //         NetEventSource.Exit(this, bytesTransferred, "ReceiveFrom");
                //     }
                //     return bytesTransferred;
                //

                // so until they upgrade their mono version, we are stuck with
                // some allocations.
                //
                // for now, let's pass the newly created on to our temp so at
                // least we reuse it next time.
                
                temp = socketAddress;

                // SocketAddress.GetHashCode() depends on SocketAddress.m_changed.
                // ReceiveFrom only sets the buffer, it does not seem to set m_changed.
                // we need to reset m_changed for two reasons:
                // * if m_changed is false, GetHashCode() returns the cahced m_hash
                //   which is '0'. that would be a problem.
                //   https://github.com/mono/mono/blob/bdd772531d379b4e78593587d15113c37edd4a64/mcs/class/referencesource/System/net/System/Net/SocketAddress.cs#L262
                // * if we have a cached m_hash, but ReceiveFrom modified the buffer
                //   then the GetHashCode() should change too. so we need to reset
                //   either way.
                //
                // the only way to do that is by _actually_ modifying the buffer:
                // https://github.com/mono/mono/blob/bdd772531d379b4e78593587d15113c37edd4a64/mcs/class/referencesource/System/net/System/Net/SocketAddress.cs#L99
                // so let's do that.
                // -> unchecked in case it's byte.Max
                unchecked
                {
                    temp[0] += 1;
                    temp[0] -= 1;
                }

                // make sure this worked.
                // at least throw an Exception to make it obvious if the trick does
                // not work anymore, in case ReceiveFrom is ever changed.
                if (temp.GetHashCode() == 0)
                    throw new Exception($"SocketAddress GetHashCode() is 0 after ReceiveFrom. Does the m_changed trick not work anymore?");

                // in the future, enable this again:
                //throw new Exception($"Socket.ReceiveFrom(): passed SocketAddress={socketAddress} but expected {temp}. This should never happen. Did ReceiveFrom() change?");
            }

            // ReceiveFrom sets seed_endpoint to the result of Create():
            // https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L1764
            // so let's return ourselves at least.
            // (seed_endpoint only seems to matter for BeginSend etc.)
            return this;
        }

        // we need to overwrite GetHashCode() for two reasons.
        // https://github.com/mono/mono/blob/bdd772531d379b4e78593587d15113c37edd4a64/mcs/class/referencesource/System/net/System/Net/IPEndPoint.cs#L160
        // * it uses m_Address. but our true SocketAddress is in m_temp.
        //   m_Address might not be set at all.
        // * m_Address.GetHashCode() allocates:
        //   https://github.com/mono/mono/blob/bdd772531d379b4e78593587d15113c37edd4a64/mcs/class/referencesource/System/net/System/Net/IPAddress.cs#L699
        public override int GetHashCode() => temp.GetHashCode();
        
        public int GetThisHashCode() => base.GetHashCode();

        // helper function to create an ACTUAL new IPEndPoint from this.
        // server needs it to store new connections as unique IPEndPoints.
        public IPEndPoint DeepCopyIPEndPoint()
        {
            // we need to create a new IPEndPoint from 'temp' SocketAddress.
            // there is no 'new IPEndPoint(SocketAddress) constructor.
            // so we need to be a bit creative...

            // allocate a placeholder IPAddress to copy
            // our SocketAddress into.
            // -> needs to be the same address family.
            IPAddress ipAddress;
            if (temp.Family == AddressFamily.InterNetworkV6)
                ipAddress = IPAddress.IPv6Any;
            else if (temp.Family == AddressFamily.InterNetwork)
                ipAddress = IPAddress.Any;
            else
                throw new Exception($"Unexpected SocketAddress family: {temp.Family}");

            // allocate a placeholder IPEndPoint
            // with the needed size form IPAddress.
            // (the real class. not NonAlloc)
            IPEndPoint placeholder = new IPEndPoint(ipAddress, 0);

            // the real IPEndPoint's .Create function can create a new IPEndPoint
            // copy from a SocketAddress.
            return (IPEndPoint)placeholder.Create(temp);
        }
#else
        
        public IPEndPointNonAlloc(long address, int port) : base(address, port)
        {
        }
        public IPEndPointNonAlloc(IPAddress address, int port) : base(address, port)
        {
        }
#endif
        
        public bool Equals(IPEndPoint ipEndPoint)
        {
            if (!object.Equals(ipEndPoint.Address, this.Address))
            {
                return false;
            }

            if (ipEndPoint.Port != this.Port)
            {
                return false;
            }

            return true;
        }
    }
    
    public static class EndPointHelper
    {
        public static IPEndPoint Clone(this EndPoint endPoint)
        {
#if UNITY
            IPEndPoint ip = ((IPEndPointNonAlloc)endPoint).DeepCopyIPEndPoint();
#else
            IPEndPoint ip = (IPEndPoint)endPoint;
            ip = new IPEndPoint(ip.Address, ip.Port);
#endif
            return ip;
        }
    }
}