namespace ET
{
    [UniqueId]
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-100000000是Core层的错误
        
        // 这里配置逻辑层的错误码
        // 100000000以上是逻辑层的错误
        // 200000000以上不抛异常  ErrorCore.IsRpcNeedThrowException
        
        //public const int ErrorExampleException = 100000000 + PackageType.Core * 1000 + 1;
        //public const int ErrorExampleNoException = 200000000 + PackageType.Core * 1000 + 1;
        public const int ERR_WithException = 100000000;
        // 小于这个Rpc会抛异常，大于这个异常的error需要自己判断处理，也就是说需要处理的错误应该要大于该值
        public const int ERR_WithoutException = 200000000;
        
        public const int ERR_Cancel = 200000000 + PackageType.Core * 1000 + 1;
        public const int ERR_Timeout = 200000000 + PackageType.Core * 1000 + 2;


        public static bool IsRpcNeedThrowException(int error)
        {
            if (error == 0)
            {
                return false;
            }
            if (error > ERR_WithoutException)
            {
                return false;
            }

            return true;
        }
    }
}