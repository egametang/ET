namespace ET
{
    public static class OpcodeRangeDefine
    {
        // 10001 - 30000 是pb，中间分成两个部分，外网pb跟内网pb
        public const ushort PbMinOpcode = 10001;
        
        public const ushort OuterMinOpcode = 10001;
        public const ushort OuterMaxOpcode = 20000;

        // 20001-30000 内网pb
        public const ushort InnerMinOpcode = 20001;
        
        public const ushort PbMaxOpcode = 30000;
        
        // 30001 - 40000 是bson，bson只用于内网
        public const ushort MongoMinOpcode = 30001;
        
        public const ushort InnerMaxOpcode = 40000;
        
        public const ushort JsonMinOpcode = 50000;
        public const ushort JsonMaxOpcode = 60000;
        
        public const ushort MaxOpcode = 60000;
    }
}