using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using FlyingWormConsole3.LiteNetLib.Utils;

namespace FlyingWormConsole3.LiteNetLib
{
    public enum NatAddressType
    {
        Internal,
        External
    }

    public interface INatPunchListener
    {
        void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token);
        void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token);
    }

    public class EventBasedNatPunchListener : INatPunchListener
    {
        public delegate void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token);
        public delegate void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token);

        public event OnNatIntroductionRequest NatIntroductionRequest;
        public event OnNatIntroductionSuccess NatIntroductionSuccess;

        void INatPunchListener.OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            if(NatIntroductionRequest != null)
                NatIntroductionRequest(localEndPoint, remoteEndPoint, token);
        }

        void INatPunchListener.OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
        {
            if (NatIntroductionSuccess != null)
                NatIntroductionSuccess(targetEndPoint, type, token);
        }
    }

    /// <summary>
    /// Module for UDP NAT Hole punching operations. Can be accessed from NetManager
    /// </summary>
    public sealed class NatPunchModule
    {
        struct RequestEventData
        {
            public IPEndPoint LocalEndPoint;
            public IPEndPoint RemoteEndPoint;
            public string Token;
        }

        struct SuccessEventData
        {
            public IPEndPoint TargetEndPoint;
            public NatAddressType Type;
            public string Token;
        }

        class NatIntroduceRequestPacket
        {
            public IPEndPoint Internal { get; set; }
            public string Token { get; set; }
        }

        class NatIntroduceResponsePacket
        {
            public IPEndPoint Internal { get; set; }
            public IPEndPoint External { get; set; }
            public string Token { get; set; }
        }

        class NatPunchPacket
        {
            public string Token { get; set; }
            public bool IsExternal { get; set; }
        }

        private readonly NetSocket _socket;
        private readonly Queue<RequestEventData> _requestEvents = new Queue<RequestEventData>();
        private readonly Queue<SuccessEventData> _successEvents = new Queue<SuccessEventData>();
        private readonly NetDataReader _cacheReader = new NetDataReader();
        private readonly NetDataWriter _cacheWriter = new NetDataWriter();
        private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor(MaxTokenLength);
        private INatPunchListener _natPunchListener;
        public const int MaxTokenLength = 256;

        internal NatPunchModule(NetSocket socket)
        {
            _socket = socket;
            _netPacketProcessor.SubscribeReusable<NatIntroduceResponsePacket>(OnNatIntroductionResponse);
            _netPacketProcessor.SubscribeReusable<NatIntroduceRequestPacket, IPEndPoint>(OnNatIntroductionRequest);
            _netPacketProcessor.SubscribeReusable<NatPunchPacket, IPEndPoint>(OnNatPunch);
        }

        internal void ProcessMessage(IPEndPoint senderEndPoint, NetPacket packet)
        {
            lock (_cacheReader)
            {
                _cacheReader.SetSource(packet.RawData, NetConstants.HeaderSize, packet.Size);
                _netPacketProcessor.ReadAllPackets(_cacheReader, senderEndPoint);
            }
        }

        public void Init(INatPunchListener listener)
        {
            _natPunchListener = listener;
        }

        private void Send<T>(T packet, IPEndPoint target) where T : class, new()
        {
            SocketError errorCode = 0;
            _cacheWriter.Reset();
            _cacheWriter.Put((byte)PacketProperty.NatMessage);
            _netPacketProcessor.Write(_cacheWriter, packet);
            _socket.SendTo(_cacheWriter.Data, 0, _cacheWriter.Length, target, ref errorCode);
        }

        public void NatIntroduce(
            IPEndPoint hostInternal,
            IPEndPoint hostExternal,
            IPEndPoint clientInternal,
            IPEndPoint clientExternal,
            string additionalInfo)
        {
            var req = new NatIntroduceResponsePacket
            {
                Token = additionalInfo
            };

            //First packet (server) send to client
            req.Internal = hostInternal;
            req.External = hostExternal;
            Send(req, clientExternal);

            //Second packet (client) send to server
            req.Internal = clientInternal;
            req.External = clientExternal;
            Send(req, hostExternal);
        }

        public void PollEvents()
        {
            if (_natPunchListener == null || (_successEvents.Count == 0 && _requestEvents.Count == 0))
                return;
            lock (_successEvents)
            {
                while (_successEvents.Count > 0)
                {
                    var evt = _successEvents.Dequeue();
                    _natPunchListener.OnNatIntroductionSuccess(
                        evt.TargetEndPoint, 
                        evt.Type,
                        evt.Token);
                }
            }
            lock (_requestEvents)
            {
                while (_requestEvents.Count > 0)
                {
                    var evt = _requestEvents.Dequeue();
                    _natPunchListener.OnNatIntroductionRequest(evt.LocalEndPoint, evt.RemoteEndPoint, evt.Token);
                }
            }
        }

        public void SendNatIntroduceRequest(string host, int port, string additionalInfo)
        {
            SendNatIntroduceRequest(NetUtils.MakeEndPoint(host, port), additionalInfo);
        }

        public void SendNatIntroduceRequest(IPEndPoint masterServerEndPoint, string additionalInfo)
        {
            //prepare outgoing data
            string networkIp = NetUtils.GetLocalIp(LocalAddrType.IPv4);
            if (string.IsNullOrEmpty(networkIp))
            {
                networkIp = NetUtils.GetLocalIp(LocalAddrType.IPv6);
            }

            Send(
                new NatIntroduceRequestPacket
                {
                    Internal = NetUtils.MakeEndPoint(networkIp, _socket.LocalPort),
                    Token = additionalInfo
                }, 
                masterServerEndPoint);
        }

        //We got request and must introduce
        private void OnNatIntroductionRequest(NatIntroduceRequestPacket req, IPEndPoint senderEndPoint)
        {
            lock (_requestEvents)
            {
                _requestEvents.Enqueue(new RequestEventData
                {
                    LocalEndPoint = req.Internal,
                    RemoteEndPoint = senderEndPoint,
                    Token = req.Token
                });
            }
        }

        //We got introduce and must punch
        private void OnNatIntroductionResponse(NatIntroduceResponsePacket req)
        {
            NetDebug.Write(NetLogLevel.Trace, "[NAT] introduction received");

            // send internal punch
            var punchPacket = new NatPunchPacket {Token = req.Token};
            Send(punchPacket, req.Internal);
            NetDebug.Write(NetLogLevel.Trace, "[NAT] internal punch sent to " + req.Internal);

            // hack for some routers
            SocketError errorCode = 0;
            _socket.Ttl = 2;
            _socket.SendTo(new[] { (byte)PacketProperty.Empty }, 0, 1, req.External, ref errorCode);

            // send external punch
            _socket.Ttl = NetConstants.SocketTTL;
            punchPacket.IsExternal = true;
            Send(punchPacket, req.External);
            NetDebug.Write(NetLogLevel.Trace, "[NAT] external punch sent to " + req.External);
        }

        //We got punch and can connect
        private void OnNatPunch(NatPunchPacket req, IPEndPoint senderEndPoint)
        {
            //Read info
            NetDebug.Write(NetLogLevel.Trace, "[NAT] punch received from {0} - additional info: {1}",
                senderEndPoint, req.Token);

            //Release punch success to client; enabling him to Connect() to Sender if token is ok
            lock (_successEvents)
            {
                _successEvents.Enqueue(new SuccessEventData
                {
                    TargetEndPoint = senderEndPoint,
                    Type = req.IsExternal ? NatAddressType.External : NatAddressType.Internal, 
                    Token = req.Token
                });
            }
        }
    }
}
