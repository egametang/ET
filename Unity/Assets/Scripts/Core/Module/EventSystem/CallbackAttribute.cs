namespace ET
{
    public class CallbackAttribute: BaseAttribute
    {
        public int Id { get; }

        public CallbackAttribute(int id = 0)
        {
            this.Id = id;
        }
    }
}