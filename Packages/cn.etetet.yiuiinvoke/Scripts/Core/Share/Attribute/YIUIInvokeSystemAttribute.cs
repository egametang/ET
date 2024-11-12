namespace ET
{
    public class YIUIInvokeSystemAttribute : BaseAttribute
    {
        public string InvokeType { get; }

        public YIUIInvokeSystemAttribute(string invokeType)
        {
            this.InvokeType = invokeType;
        }
    }
}