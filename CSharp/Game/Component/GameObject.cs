using Common.Component;

namespace Component
{
    public class GameObject: Object
    {
        private readonly BuffManager buffManager = new BuffManager();

        public int Type { get; set; }

        public BuffManager BuffManager
        {
            get
            {
                return this.buffManager;
            }
        }
    }
}