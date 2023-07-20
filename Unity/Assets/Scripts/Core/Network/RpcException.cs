using System;

namespace ET
{
    /// <summary>
    /// RPC异常,带ErrorCode
    /// </summary>
    public class RpcException: Exception
    {
        public int Error
        {
            get;
        }

        public RpcException(int error, string message): base(message)
        {
            this.Error = error;
        }

        public override string ToString()
        {
            return $"Error: {this.Error}\n{base.ToString()}";
        }
    }
}