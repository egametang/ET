using Common.Base;

namespace Model
{
    public class Buff: Object
    {
        private BuffType type;

        public BuffType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}