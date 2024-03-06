using System;
using System.Collections.Generic;

namespace ET
{
    public class ChildrenCollection : SortedDictionary<long, Entity>, IPool
    {
        public static ChildrenCollection Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ChildrenCollection>(isFromPool);
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