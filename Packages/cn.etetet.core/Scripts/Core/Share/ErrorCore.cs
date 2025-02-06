namespace ET
{
    [UniqueId]
    public static partial class ErrorCore
    {
        public const int ERR_Success = 0;
        
        public const int ERR_MyErrorCode = 110000;
        
        public const int ERR_KcpConnectTimeout = 100205;
        public const int ERR_KcpAcceptTimeout = 100206;
        public const int ERR_KcpReadWriteTimeout = 100207;
        public const int ERR_PeerDisconnect = 100208;
        public const int ERR_SocketCantSend = 100209;
        public const int ERR_SocketError = 100210;
        public const int ERR_KcpWaitSendSizeTooLarge = 100211;
        public const int ERR_KcpCreateError = 100212;
        public const int ERR_SendMessageNotFoundTChannel = 100213;
        public const int ERR_TChannelRecvError = 100214;
        public const int ERR_MessageSocketParserError = 100215;
        public const int ERR_KcpNotFoundChannel = 100216;

        public const int ERR_WebsocketSendError = 100217;
        public const int ERR_WebsocketPeerReset = 100218;
        public const int ERR_WebsocketMessageTooBig = 100219;
        public const int ERR_WebsocketRecvError = 100220;
        
        public const int ERR_KcpReadNotSame = 100230;
        public const int ERR_KcpSplitError = 100231;
        public const int ERR_KcpSplitCountError = 100232;

        public const int ERR_PacketParserError = 110005;
        public const int ERR_WebsocketConnectError = 110304;
        
        // 110000 以上，避免跟SocketError冲突
    }
}