using System;

namespace ET
{
    public struct EntityRef<T> where T: Entity
    {
        private long instanceId;
        // 使用WeakReference，这样不会导致entity dispose了却无法gc的问题
        // 不过暂时没有测试WeakReference的性能
        private readonly WeakReference<T> weakRef;

        private EntityRef(T t)
        {
            this.instanceId = t.InstanceId;
            this.weakRef = new WeakReference<T>(t);
        }
        
        private T UnWrap
        {
            get
            {
                if (this.instanceId == 0)
                {
                    return null;
                }

                if (!this.weakRef.TryGetTarget(out T entity))
                {
                    this.instanceId = 0;
                    return null;
                }

                if (entity.InstanceId != this.instanceId)
                {
                    this.instanceId = 0;
                    return null;
                }
                return entity;
            }
        }
        
        public static implicit operator EntityRef<T>(T t)
        {
            return new EntityRef<T>(t);
        }

        public static implicit operator T(EntityRef<T> v)
        {
            return v.UnWrap;
        }
    }
}