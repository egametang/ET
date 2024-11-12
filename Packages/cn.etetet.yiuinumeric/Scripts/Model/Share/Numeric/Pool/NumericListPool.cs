using System.Collections.Generic;

namespace ET
{
    [EnableClass]
    public class NumericListPool<K> : List<K>, IPool
    {
        public void Dispose()
        {
            this.Clear();
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}
