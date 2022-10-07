namespace ET
{
    public class ConsoleHandlerAttribute: BaseAttribute
    {
        public string Mode { get; }

        public ConsoleHandlerAttribute(string mode)
        {
            this.Mode = mode;
        }
    }
}