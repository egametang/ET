#if UNITY_5_3_OR_NEWER
#define UNITY
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine;
#endif
#endif
#if NETSTANDARD || NETCOREAPP
using System.Runtime.InteropServices;
#endif

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FlyingWormConsole3.LiteNetLib
{
#if UNITY_IOS && !UNITY_EDITOR
    public class UnitySocketFix : MonoBehaviour
    {
        internal IPAddress BindAddrIPv4;
        internal IPAddress BindAddrIPv6;
        internal bool Reuse;
        internal IPv6Mode IPv6;
        internal int Port;
        internal bool Paused;
        internal NetSocket Socket;
        internal bool ManualMode;

        private void Update()
        {
            if (Socket == null)
                Destroy(gameObject);
        }

        private void OnApplicationPause(bool pause)
        {
            if (Socket == null)
                return;
            if (pause)
            {
                Paused = true;
                Socket.Close(true);
            }
            else if (Paused)
            {
                if (!Socket.Bind(BindAddrIPv4, BindAddrIPv6, Port, Reuse, IPv6, ManualMode))
                {
                    NetDebug.WriteError("[S] Cannot restore connection \"{0}\",\"{1}\" port {2}", BindAddrIPv4, BindAddrIPv6, Port);
                    Socket.OnErrorRestore();
                }
            }
        }
    }
#endif

    internal sealed class NetSocket
    {
        public const int ReceivePollingTime = 500000; //0.5 second

        private Socket _udpSocketv4;
        private Socket _udpSocketv6;
        private Thread _threadv4;
        private Thread _threadv6;
        private IPEndPoint _bufferEndPointv4;
        private IPEndPoint _bufferEndPointv6;

        private readonly NetManager _listener;

        private const int SioUdpConnreset = -1744830452; //SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12
        private static readonly IPAddress MulticastAddressV6 = IPAddress.Parse("ff02::1");
        internal static readonly bool IPv6Support;
#if UNITY_IOS && !UNITY_EDITOR
        private UnitySocketFix _unitySocketFix;

        public void OnErrorRestore()
        {
            Close(false);
            _listener.OnMessageReceived(null, SocketError.NotConnected, new IPEndPoint(0,0));
        }
#endif
        public int LocalPort { get; private set; }
        public volatile bool IsRunning;

        public short Ttl
        {
            get
            {
#if UNITY_SWITCH
                return 0;
#else
                if (_udpSocketv4.AddressFamily == AddressFamily.InterNetworkV6)
                    return (short)_udpSocketv4.GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.HopLimit);
                return _udpSocketv4.Ttl;
#endif
            }
            set
            {
#if !UNITY_SWITCH
                if (_udpSocketv4.AddressFamily == AddressFamily.InterNetworkV6)
                    _udpSocketv4.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.HopLimit, value);
                else
                    _udpSocketv4.Ttl = value;
#endif
            }
        }

        static NetSocket()
        {
#if DISABLE_IPV6 || (!UNITY_EDITOR && ENABLE_IL2CPP && !UNITY_2018_3_OR_NEWER)
            IPv6Support = false;
#elif !UNITY_2019_1_OR_NEWER && !UNITY_2018_4_OR_NEWER && (!UNITY_EDITOR && ENABLE_IL2CPP && UNITY_2018_3_OR_NEWER)
            string version = UnityEngine.Application.unityVersion;
            IPv6Support = Socket.OSSupportsIPv6 && int.Parse(version.Remove(version.IndexOf('f')).Split('.')[2]) >= 6;
#elif UNITY_2018_2_OR_NEWER
            IPv6Support = Socket.OSSupportsIPv6;
#elif UNITY
#pragma warning disable 618
            IPv6Support = Socket.SupportsIPv6;
#pragma warning restore 618
#else
            IPv6Support = Socket.OSSupportsIPv6;
#endif
        }

        public NetSocket(NetManager listener)
        {
            _listener = listener;
        }

        private bool IsActive()
        {
#if UNITY_IOS && !UNITY_EDITOR
            var unitySocketFix = _unitySocketFix; //save for multithread
            if (unitySocketFix != null && unitySocketFix.Paused)
                return false;
#endif
            return IsRunning;
        }

        private bool ProcessError(SocketException ex, EndPoint bufferEndPoint)
        {
            switch (ex.SocketErrorCode)
            {
#if UNITY_IOS && !UNITY_EDITOR
                case SocketError.NotConnected:
#endif
                case SocketError.Interrupted:
                case SocketError.NotSocket:
                    return true;
                case SocketError.ConnectionReset:
                case SocketError.MessageSize:
                case SocketError.TimedOut:
                    NetDebug.Write(NetLogLevel.Trace, "[R]Ignored error: {0} - {1}",
                        (int)ex.SocketErrorCode, ex.ToString());
                    break;
                default:
                    NetDebug.WriteError("[R]Error code: {0} - {1}", (int)ex.SocketErrorCode,
                        ex.ToString());
                    _listener.OnMessageReceived(null, ex.SocketErrorCode, (IPEndPoint)bufferEndPoint);
                    break;
            }
            return false;
        }

        public void ManualReceive()
        {
            ManualReceive(_udpSocketv4, _bufferEndPointv4);
            if (_udpSocketv6 != null && _udpSocketv6 != _udpSocketv4)
                ManualReceive(_udpSocketv6, _bufferEndPointv6);
        }

        private bool ManualReceive(Socket socket, EndPoint bufferEndPoint)
        {
            //Reading data
            try
            {
                int available = socket.Available;
                if (available == 0)
                    return false;
                while (available > 0)
                {
                    var packet = _listener.NetPacketPool.GetPacket(NetConstants.MaxPacketSize);
                    packet.Size = socket.ReceiveFrom(packet.RawData, 0, NetConstants.MaxPacketSize, SocketFlags.None,
                        ref bufferEndPoint);
                    NetDebug.Write(NetLogLevel.Trace, "[R]Received data from {0}, result: {1}", bufferEndPoint.ToString(), packet.Size);
                    _listener.OnMessageReceived(packet, 0, (IPEndPoint)bufferEndPoint);
                    available -= packet.Size;
                }
            }
            catch (SocketException ex)
            {
                return ProcessError(ex, bufferEndPoint);
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
            return false;
        }

        private void ReceiveLogic(object state)
        {
            Socket socket = (Socket)state;
            EndPoint bufferEndPoint = new IPEndPoint(socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);

            while (IsActive())
            {
                NetPacket packet;

                //Reading data
                try
                {
                    if (socket.Available == 0 && !socket.Poll(ReceivePollingTime, SelectMode.SelectRead))
                        continue;
                    packet = _listener.NetPacketPool.GetPacket(NetConstants.MaxPacketSize);
                    packet.Size = socket.ReceiveFrom(packet.RawData, 0, NetConstants.MaxPacketSize, SocketFlags.None,
                        ref bufferEndPoint);
                }
                catch (SocketException ex)
                {
                    if (ProcessError(ex, bufferEndPoint))
                        return;
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                //All ok!
                NetDebug.Write(NetLogLevel.Trace, "[R]Received data from {0}, result: {1}", bufferEndPoint.ToString(), packet.Size);
                _listener.OnMessageReceived(packet, 0, (IPEndPoint)bufferEndPoint);
            }
        }

        public bool Bind(IPAddress addressIPv4, IPAddress addressIPv6, int port, bool reuseAddress, IPv6Mode ipv6Mode, bool manualMode)
        {
            if (IsActive())
                return false;
            bool dualMode = ipv6Mode == IPv6Mode.DualMode && IPv6Support;

            _udpSocketv4 = new Socket(
                dualMode ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, 
                SocketType.Dgram, 
                ProtocolType.Udp);

            if (!BindSocket(_udpSocketv4, new IPEndPoint(dualMode ? addressIPv6 : addressIPv4, port), reuseAddress, ipv6Mode))
                return false;

            LocalPort = ((IPEndPoint) _udpSocketv4.LocalEndPoint).Port;

#if UNITY_IOS && !UNITY_EDITOR
            if (_unitySocketFix == null)
            {
                var unityFixObj = new GameObject("LiteNetLib_UnitySocketFix");
                GameObject.DontDestroyOnLoad(unityFixObj);
                _unitySocketFix = unityFixObj.AddComponent<UnitySocketFix>();
                _unitySocketFix.Socket = this;
                _unitySocketFix.BindAddrIPv4 = addressIPv4;
                _unitySocketFix.BindAddrIPv6 = addressIPv6;
                _unitySocketFix.Reuse = reuseAddress;
                _unitySocketFix.Port = LocalPort;
                _unitySocketFix.IPv6 = ipv6Mode;
                _unitySocketFix.ManualMode = manualMode;
            }
            else
            {
                _unitySocketFix.Paused = false;
            }
#endif
            if (dualMode)
                _udpSocketv6 = _udpSocketv4;

            IsRunning = true;
            if (!manualMode)
            {
                _threadv4 = new Thread(ReceiveLogic)
                {
                    Name = "SocketThreadv4(" + LocalPort + ")",
                    IsBackground = true
                };
                _threadv4.Start(_udpSocketv4);
            }
            else
            {
                _bufferEndPointv4 = new IPEndPoint(IPAddress.Any, 0);
            }

            //Check IPv6 support
            if (!IPv6Support || ipv6Mode != IPv6Mode.SeparateSocket)
                return true;

            _udpSocketv6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            //Use one port for two sockets
            if (BindSocket(_udpSocketv6, new IPEndPoint(addressIPv6, LocalPort), reuseAddress, ipv6Mode))
            {
                if (manualMode)
                {
                    _bufferEndPointv6 = new IPEndPoint(IPAddress.IPv6Any, 0);
                }
                else
                {
                    _threadv6 = new Thread(ReceiveLogic)
                    {
                        Name = "SocketThreadv6(" + LocalPort + ")",
                        IsBackground = true
                    };
                    _threadv6.Start(_udpSocketv6);
                }

            }

            return true;
        }

        private bool BindSocket(Socket socket, IPEndPoint ep, bool reuseAddress, IPv6Mode ipv6Mode)
        {
            //Setup socket
            socket.ReceiveTimeout = 500;
            socket.SendTimeout = 500;
            socket.ReceiveBufferSize = NetConstants.SocketBufferSize;
            socket.SendBufferSize = NetConstants.SocketBufferSize;
#if !UNITY || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#if NETSTANDARD || NETCOREAPP
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#endif
            try
            {
                socket.IOControl(SioUdpConnreset, new byte[] { 0 }, null);
            }
            catch
            {
                //ignored
            }
#endif

            try
            {
                socket.ExclusiveAddressUse = !reuseAddress;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress);
            }
            catch
            {
                //Unity with IL2CPP throws an exception here, it doesn't matter in most cases so just ignore it
            }
            if (socket.AddressFamily == AddressFamily.InterNetwork)
            {
                Ttl = NetConstants.SocketTTL;

#if NETSTANDARD || NETCOREAPP
                if(!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
#endif
                try { socket.DontFragment = true; }
                catch (SocketException e)
                {
                    NetDebug.WriteError("[B]DontFragment error: {0}", e.SocketErrorCode);
                }

                try { socket.EnableBroadcast = true; }
                catch (SocketException e)
                {
                    NetDebug.WriteError("[B]Broadcast error: {0}", e.SocketErrorCode);
                }
            }
            else //IPv6 specific
            {
                if (ipv6Mode == IPv6Mode.DualMode)
                {
                    try
                    {
                        //Disable IPv6 only mode
                        socket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                    }
                    catch(Exception e)
                    {
                        NetDebug.WriteError("[B]Bind exception (dualmode setting): {0}", e.ToString());
                    }
                }
            }

            //Bind
            try
            {
                socket.Bind(ep);
                NetDebug.Write(NetLogLevel.Trace, "[B]Successfully binded to port: {0}", ((IPEndPoint)socket.LocalEndPoint).Port);

                //join multicast
                if (socket.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    try
                    {
#if !UNITY
                        socket.SetSocketOption(
                            SocketOptionLevel.IPv6,
                            SocketOptionName.AddMembership,
                            new IPv6MulticastOption(MulticastAddressV6));
#endif
                    }
                    catch (Exception)
                    {
                        // Unity3d throws exception - ignored
                    }
                }
            }
            catch (SocketException bindException)
            {
                switch (bindException.SocketErrorCode)
                {
                    //IPv6 bind fix
                    case SocketError.AddressAlreadyInUse:
                        if (socket.AddressFamily == AddressFamily.InterNetworkV6 && ipv6Mode != IPv6Mode.DualMode)
                        {
                            try
                            {
                                //Set IPv6Only
                                socket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, true);
                                socket.Bind(ep);
                            }
#if UNITY_2018_3_OR_NEWER
                            catch (SocketException ex)
                            {

                                //because its fixed in 2018_3
                                NetDebug.WriteError("[B]Bind exception: {0}, errorCode: {1}", ex.ToString(), ex.SocketErrorCode);
#else
                            catch(SocketException)
                            {
#endif
                                return false;
                            }
                            return true;
                        }
                        break;
                    //hack for iOS (Unity3D)
                    case SocketError.AddressFamilyNotSupported:
                        return true;
                }
                NetDebug.WriteError("[B]Bind exception: {0}, errorCode: {1}", bindException.ToString(), bindException.SocketErrorCode);
                return false;
            }
            return true;
        }

        public bool SendBroadcast(byte[] data, int offset, int size, int port)
        {
            if (!IsActive())
                return false;
            bool broadcastSuccess = false;
            bool multicastSuccess = false;
            try
            {
                broadcastSuccess = _udpSocketv4.SendTo(
                             data,
                             offset,
                             size,
                             SocketFlags.None,
                             new IPEndPoint(IPAddress.Broadcast, port)) > 0;

                if (_udpSocketv6 != null)
                {
                    multicastSuccess = _udpSocketv6.SendTo(
                                                data,
                                                offset,
                                                size,
                                                SocketFlags.None,
                                                new IPEndPoint(MulticastAddressV6, port)) > 0;
                }
            }
            catch (Exception ex)
            {
                NetDebug.WriteError("[S][MCAST]" + ex);
                return broadcastSuccess;
            }
            return broadcastSuccess || multicastSuccess;
        }

        public int SendTo(byte[] data, int offset, int size, IPEndPoint remoteEndPoint, ref SocketError errorCode)
        {
            if (!IsActive())
                return 0;
            try
            {
                var socket = _udpSocketv4;
                if (remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 && IPv6Support)
                    socket = _udpSocketv6;
                int result = socket.SendTo(data, offset, size, SocketFlags.None, remoteEndPoint);
                NetDebug.Write(NetLogLevel.Trace, "[S]Send packet to {0}, result: {1}", remoteEndPoint, result);
                return result;
            }
            catch (SocketException ex)
            {
                switch (ex.SocketErrorCode)
                {
                    case SocketError.NoBufferSpaceAvailable:
                    case SocketError.Interrupted:
                        return 0;
                    case SocketError.MessageSize: //do nothing              
                        break;
                    default:
                        NetDebug.WriteError("[S]" + ex);
                        break;
                }
                errorCode = ex.SocketErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                NetDebug.WriteError("[S]" + ex);
                return -1;
            }
        }

        public void Close(bool suspend)
        {
            if (!suspend)
            {
                IsRunning = false;
#if UNITY_IOS && !UNITY_EDITOR
                _unitySocketFix.Socket = null;
                _unitySocketFix = null;
#endif
            }
            //cleanup dual mode
            if (_udpSocketv4 == _udpSocketv6)
                _udpSocketv6 = null;

            if (_udpSocketv4 != null)
                _udpSocketv4.Close();
            if (_udpSocketv6 != null)
                _udpSocketv6.Close();
            _udpSocketv4 = null;
            _udpSocketv6 = null;

            if (_threadv4 != null && _threadv4 != Thread.CurrentThread)
                _threadv4.Join();
            if (_threadv6 != null && _threadv6 != Thread.CurrentThread)
                _threadv6.Join();
            _threadv4 = null;
            _threadv6 = null;
        }
    }
}
