using System;
using System.Collections.Generic;

namespace ET
{
    public class ComponentsCollection : SortedDictionary<long, Entity>, IPool
    {
        public static ComponentsCollection Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ComponentsCollection>(isFromPool);
        }
        
        public bool IsFromPool { get; set; }

        public void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }
            
            this.IsFromPool = false;
            this.Clear();
            
            ObjectPool.Recycle(this);
        }
    }
}