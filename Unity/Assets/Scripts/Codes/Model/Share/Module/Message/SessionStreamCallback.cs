using System.IO;

namespace ET
{
    public static class SessionStreamCallbackId
    {
        public const int NetClient = 1;
        public const int NetServer = 2;
        public const int NetInner = 3;
    }
    
    public struct SessionStreamCallback: ICallback
    {
        public int Id { get; set; }

        public Session Session;

        public long ActorId;
        
        public object Message;
    }
}