using System.IO;

namespace ET
{
    public static class SessionStreamCallbackId
    {
        public const int SessionStreamDispatcherClientOuter = 1;
        public const int SessionStreamDispatcherServerOuter = 2;
        public const int SessionStreamDispatcherServerInner = 3;
    }
    
    public struct SessionStreamCallback: ICallback
    {
        public int Id { get; set; }

        public Session Session;

        public MemoryStream MemoryStream;
    }
}