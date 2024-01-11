using System.Net;
using System.Threading;
using FlyingWormConsole3.LiteNetLib.Utils;

namespace FlyingWormConsole3.LiteNetLib
{
    internal enum ConnectionRequestResult
    {
        None,
        Accept,
        Reject,
        RejectForce
    }

    public class ConnectionRequest
    {
        private readonly NetManager _listener;
        private int _used;

        public readonly NetDataReader Data;

        internal ConnectionRequestResult Result { get; private set; }
        internal long ConnectionTime;
        internal byte ConnectionNumber;
        public readonly IPEndPoint RemoteEndPoint;

        private bool TryActivate()
        {
            return Interlocked.CompareExchange(ref _used, 1, 0) == 0;
        }

        internal void UpdateRequest(NetConnectRequestPacket connRequest)
        {
            if (connRequest.ConnectionTime >= ConnectionTime)
            {
                ConnectionTime = connRequest.ConnectionTime;
                ConnectionNumber = connRequest.ConnectionNumber;
            }
        }

        internal ConnectionRequest(
            long connectionId,
            byte connectionNumber,
            NetDataReader netDataReader,
            IPEndPoint endPoint,
            NetManager listener)
        {
            ConnectionTime = connectionId;
            ConnectionNumber = connectionNumber;
            RemoteEndPoint = endPoint;
            Data = netDataReader;
            _listener = listener;
        }

        public NetPeer AcceptIfKey(string key)
        {
            if (!TryActivate())
                return null;
            try
            {
                if (Data.GetString() == key)
                    Result = ConnectionRequestResult.Accept;
            }
            catch
            {
                NetDebug.WriteError("[AC] Invalid incoming data");
            }
            if (Result == ConnectionRequestResult.Accept)
                return _listener.OnConnectionSolved(this, null, 0, 0);

            Result = ConnectionRequestResult.Reject;
            _listener.OnConnectionSolved(this, null, 0, 0);
            return null;
        }

        /// <summary>
        /// Accept connection and get new NetPeer as result
        /// </summary>
        /// <returns>Connected NetPeer</returns>
        public NetPeer Accept()
        {
            if (!TryActivate())
                return null;
            Result = ConnectionRequestResult.Accept;
            return _listener.OnConnectionSolved(this, null, 0, 0);
        }
        
        public void Reject(byte[] rejectData, int start, int length, bool force)
        {
            if (!TryActivate())
                return;
            Result = force ? ConnectionRequestResult.RejectForce : ConnectionRequestResult.Reject;
            _listener.OnConnectionSolved(this, rejectData, start, length);
        }

        public void Reject(byte[] rejectData, int start, int length)
        {
            Reject(rejectData, start, length, false);
        }


        public void RejectForce(byte[] rejectData, int start, int length)
        {
            Reject(rejectData, start, length, true);
        }

        public void RejectForce()
        {
            Reject(null, 0, 0, true);
        }

        public void RejectForce(byte[] rejectData)
        {
            Reject(rejectData, 0, rejectData.Length, true);
        }

        public void RejectForce(NetDataWriter rejectData)
        {
            Reject(rejectData.Data, 0, rejectData.Length, true);
        }

        public void Reject()
        {
            Reject(null, 0, 0, false);
        }

        public void Reject(byte[] rejectData)
        {
            Reject(rejectData, 0, rejectData.Length, false);
        }

        public void Reject(NetDataWriter rejectData)
        {
            Reject(rejectData.Data, 0, rejectData.Length, false);
        }
    }
}
