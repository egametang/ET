namespace ET
{
    public class IdValueGenerater: Singleton<IdValueGenerater>
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
    }
}