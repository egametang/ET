using System;

namespace ET
{
    //文档:https://lib9kmxvq7k.feishu.cn/wiki/TpyYwbWIUizhfKkcubocTZgInse
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class YIUIInvokeAttribute : BaseAttribute
    {
        public string InvokeType { get; }

        //如果有参数必须是Const
        public YIUIInvokeAttribute(string invokeType)
        {
            this.InvokeType = invokeType;
        }

        //没有参数则SG读取的是方法名
        public YIUIInvokeAttribute()
        {
        }
    }
}