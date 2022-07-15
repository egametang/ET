namespace ET
{
    public class CallbackAttribute: BaseAttribute
    {
        public int Type { get; }

        public CallbackAttribute(int type)
        {
            this.Type = type;
        }
    }
}