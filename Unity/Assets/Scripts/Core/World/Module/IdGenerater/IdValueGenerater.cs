namespace ET
{
    public class IdValueGenerater: Singleton<IdValueGenerater>, ISingletonAwake
    {
        private uint value;

        public uint Value
        {
            get
            {
                lock (this)
                {
                    if (++this.value > IdGenerater.Mask20bit - 1)
                    {
                        this.value = 0;
                    }
                    return this.value;
                }
            }
        }

        public void Awake()
        {
            
        }
    }
}