using System;

namespace ET
{
    public struct EntityRef<T> where T: Entity
    {
        private readonly long instanceId;
        private T entity;

        private EntityRef(T t)
        {
            if (t == null)
            {
                this.instanceId = 0;
                this.entity = null;
                return;
            }
            this.instanceId = t.InstanceId;
            this.entity = t;
        }
        
        private T UnWrap
        {
            get
            {
                if (this.entity == null)
                {
                    return null;
                }
                if (this.entity.InstanceId != this.instanceId)
                {
                    // 这里instanceId变化了，设置为null，解除引用，好让runtime去gc
                    this.entity = null;
                }
                return this.entity;
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
    
    
    public struct EntityWeakRef<T> where T: Entity
    {
        private long instanceId;
        // 使用WeakReference，这样不会导致entity dispose了却无法gc的问题
        // 不过暂时没有测试WeakReference的性能
        private readonly WeakReference<T> weakRef;

        private EntityWeakRef(T t)
        {
            if (t == null)
            {
                this.instanceId = 0;
                this.weakRef = null;
                return;
            }
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
        
        public static implicit operator EntityWeakRef<T>(T t)
        {
            return new EntityWeakRef<T>(t);
        }

        public static implicit operator T(EntityWeakRef<T> v)
        {
            return v.UnWrap;
        }
    }
}