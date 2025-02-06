namespace ET
{
    public class InvokeAttribute: BaseAttribute
    {
        public long Type { get; }

        public InvokeAttribute(long type = 0)
        {
            this.Type = type;
        }
    }
}