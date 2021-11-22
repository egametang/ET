using System.Collections.Generic;

namespace ET
{
    public class HashSetComponent<T>: Object
    {
        private bool isDispose;
        
        public static HashSetComponent<T> Create()
        {
            HashSetComponent<T> hashSetComponent = ObjectPool.Instance.Fetch<HashSetComponent<T>>();
            hashSetComponent.isDispose = false;
            return hashSetComponent;
        }
        
        public HashSet<T> Set = new HashSet<T>();

        public override void Dispose()
        {
            if (this.isDispose)
            {
                return;
            }

            this.isDispose = true;
            
            base.Dispose();
            
            this.Set.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}