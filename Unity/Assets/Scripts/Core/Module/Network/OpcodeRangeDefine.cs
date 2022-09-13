namespace ET
{
    public static class OpcodeRangeDefine
    {
        public const ushort OuterMinOpcode = 10001;
        public const ushort OuterMaxOpcode = 20000;

        // 20001-30000 内网pb
        public const ushort InnerMinOpcode = 20001;
        public const ushort InnerMaxOpcode = 40000;
        
        public const ushort MaxOpcode = 60000;
    }
}