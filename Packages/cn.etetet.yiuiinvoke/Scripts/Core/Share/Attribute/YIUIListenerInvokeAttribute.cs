using System;

namespace ET
{
    //文档 https://lib9kmxvq7k.feishu.cn/wiki/XXzrwRGgti52s4kYdqFccswAnXe
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class YIUIListenerInvokeAttribute : BaseAttribute
    {
        public string InvokeType { get; }

        /// <summary>
        /// 执行优先级
        /// 小于0 则在Invoke之前执行
        /// 大于等于0 则在Invoke之后执行
        /// 同级越小的越先执行
        /// </summary>
        public int Priority { get; }

        //有此特性将会被SG 自动生成的代码
        public YIUIListenerInvokeAttribute(string invokeType, int priority = 0)
        {
            this.InvokeType = invokeType;
            this.Priority   = priority;
        }
    }
}