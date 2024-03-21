namespace ET
{
    public static class ResponseHelper
    {
        /// <summary>
        /// 返回消息是否存在错误
        /// </summary>
        public static bool HasError(this IResponse response)
        {
            return response.Error != ErrorCode.ERR_Success;
        }
    }
}