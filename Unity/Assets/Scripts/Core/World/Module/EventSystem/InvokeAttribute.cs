namespace ET
{
    public class InvokeAttribute: BaseAttribute
    {
        public int Type { get; }

        public InvokeAttribute(int type = 0)
        {
            this.Type = type;
        }
    }
}