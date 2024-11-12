using System.Collections.Generic;

namespace ET
{
    [EnableClass]
    public class NumericDictionaryPool<K, V> : Dictionary<K, V>, IPool
    {
        public void Dispose()
        {
            this.Clear();
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}
