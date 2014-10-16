using Common.Base;

namespace Model
{
    public class Buff: Object
    {
        private int ConfigId { get; set; }

        public Buff(int configId)
        {
            this.ConfigId = configId;
        }

        public BuffConfig Config
        {
            get
            {
                return World.Instance.ConfigManager.Get<BuffConfig>(this.ConfigId);
            }
        }
    }
}