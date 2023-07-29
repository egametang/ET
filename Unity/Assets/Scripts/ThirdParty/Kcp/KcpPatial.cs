namespace ET
{
    public partial class Kcp
    {
        public const int OneM = 1024 * 1024;
        public const int InnerMaxWaitSize = 1024 * 1024;
        public const int OuterMaxWaitSize = 1024 * 1024;

        public struct SegmentHead
        {
            public uint conv;     
            public byte cmd;
            public byte frg;
            public ushort wnd;      
            public uint ts;     
            public uint sn;       
            public uint una;
            public uint len;
        }
    }
    
    
}