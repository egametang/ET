using System.Net;
using System.Net.Sockets;
using FlyingWormConsole3.LiteNetLib.Utils;

namespace FlyingWormConsole3.LiteNetLib
{
    /// <summary>
    /// Type of message that you receive in OnNetworkReceiveUnconnected event
    /// </summary>
    public enum UnconnectedMessageType
    {
        BasicMessage,
        Broadcast
    }

    /// <summary>
    /// Disconnect reason that you receive in OnPeerDisconnected event
    /// </summary>
    public enum DisconnectReason
    {
        ConnectionFailed,
        Timeout,
        HostUnreachable,
        NetworkUnreachable,
        RemoteConnectionClose,
        DisconnectPeerCalled,
        ConnectionRejected,
        InvalidProtocol,
        UnknownHost,
        Reconnect,
        PeerToPeerConnection
    }

    /// <summary>
    /// Additional information about disconnection
    /// </summary>
    public struct DisconnectInfo
    {
        /// <summary>
        /// Additional info why peer disconnected
        /// </summary>
        public DisconnectReason Reason;

        /// <summary>
        /// Error code (if reason is SocketSendError or SocketReceiveError)
        /// </summary>
        public SocketError SocketErrorCode;

        /// <summary>
        /// Additional data that can be accessed (only if reason is RemoteConnectionClose)
        /// </summary>
        public NetPacketReader AdditionalData;
    }

    public interface INetEventListener
    {
        /// <summary>
        /// New remote peer connected to host, or client connected to remote host
        /// </summary>
        /// <param name="peer">Connected peer object</param>
        void OnPeerConnected(NetPeer peer);

        /// <summary>
        /// Peer disconnected
        /// </summary>
        /// <param name="peer">disconnected peer</param>
        /// <param name="disconnectInfo">additional info about reason, errorCode or data received with disconnect message</param>
        void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);

        /// <summary>
        /// Network error (on send or receive)
        /// </summary>
        /// <param name="endPoint">From endPoint (can be null)</param>
        /// <param name="socketError">Socket error</param>
        void OnNetworkError(IPEndPoint endPoint, SocketError socketError);

        /// <summary>
        /// Received some data
        /// </summary>
        /// <param name="peer">From peer</param>
        /// <param name="reader">DataReader containing all received data</param>
        /// <param name="deliveryMethod">Type of received packet</param>
        void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod);

        /// <summary>
        /// Received unconnected message
        /// </summary>
        /// <param name="remoteEndPoint">From address (IP and Port)</param>
        /// <param name="reader">Message data</param>
        /// <param name="messageType">Message type (simple, discovery request or response)</param>
        void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);

        /// <summary>
        /// Latency information updated
        /// </summary>
        /// <param name="peer">Peer with updated latency</param>
        /// <param name="latency">latency value in milliseconds</param>
        void OnNetworkLatencyUpdate(NetPeer peer, int latency);

        /// <summary>
        /// On peer connection requested
        /// </summary>
        /// <param name="request">Request information (EndPoint, internal id, additional data)</param>
        void OnConnectionRequest(ConnectionRequest request);
    }

    public interface IDeliveryEventListener
    {
        /// <summary>
        /// On reliable message delivered
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="userData"></param>
        void OnMessageDelivered(NetPeer peer, object userData);
    }

    public interface INtpEventListener
    {
        /// <summary>
        /// Ntp response
        /// </summary>
        /// <param name="packet"></param>
        void OnNtpResponse(NtpPacket packet);
    }

    public class EventBasedNetListener : INetEventListener, IDeliveryEventListener, INtpEventListener
    {
        public delegate void OnPeerConnected(NetPeer peer);
        public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
        public delegate void OnNetworkError(IPEndPoint endPoint, SocketError socketError);
        public delegate void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod);
        public delegate void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
        public delegate void OnNetworkLatencyUpdate(NetPeer peer, int latency);
        public delegate void OnConnectionRequest(ConnectionRequest request);
        public delegate void OnDeliveryEvent(NetPeer peer, object userData);
        public delegate void OnNtpResponseEvent(NtpPacket packet);

        public event OnPeerConnected PeerConnectedEvent;
        public event OnPeerDisconnected PeerDisconnectedEvent;
        public event OnNetworkError NetworkErrorEvent;
        public event OnNetworkReceive NetworkReceiveEvent;
        public event OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;
        public event OnNetworkLatencyUpdate NetworkLatencyUpdateEvent;
        public event OnConnectionRequest ConnectionRequestEvent;
        public event OnDeliveryEvent DeliveryEvent;
        public event OnNtpResponseEvent NtpResponseEvent;

        public void ClearPeerConnectedEvent()
        {
            PeerConnectedEvent = null;
        }

        public void ClearPeerDisconnectedEvent()
        {
            PeerDisconnectedEvent = null;
        }

        public void ClearNetworkErrorEvent()
        {
            NetworkErrorEvent = null;
        }

        public void ClearNetworkReceiveEvent()
        {
            NetworkReceiveEvent = null;
        }

        public void ClearNetworkReceiveUnconnectedEvent()
        {
            NetworkReceiveUnconnectedEvent = null;
        }

        public void ClearNetworkLatencyUpdateEvent()
        {
            NetworkLatencyUpdateEvent = null;
        }

        public void ClearConnectionRequestEvent()
        {
            ConnectionRequestEvent = null;
        }

        public void ClearDeliveryEvent()
        {
            DeliveryEvent = null;
        }

        public void ClearNtpResponseEvent()
        {
            NtpResponseEvent = null;
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            if (PeerConnectedEvent != null)
                PeerConnectedEvent(peer);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (PeerDisconnectedEvent != null)
                PeerDisconnectedEvent(peer, disconnectInfo);
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            if (NetworkErrorEvent != null)
                NetworkErrorEvent(endPoint, socketErrorCode);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (NetworkReceiveEvent != null)
                NetworkReceiveEvent(peer, reader, deliveryMethod);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (NetworkReceiveUnconnectedEvent != null)
                NetworkReceiveUnconnectedEvent(remoteEndPoint, reader, messageType);
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            if (NetworkLatencyUpdateEvent != null)
                NetworkLatencyUpdateEvent(peer, latency);
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            if (ConnectionRequestEvent != null)
                ConnectionRequestEvent(request);
        }

        void IDeliveryEventListener.OnMessageDelivered(NetPeer peer, object userData)
        {
            if (DeliveryEvent != null)
                DeliveryEvent(peer, userData);
        }

        void INtpEventListener.OnNtpResponse(NtpPacket packet)
        {
            if (NtpResponseEvent != null)
                NtpResponseEvent(packet);
        }
    }
}
