using System;

namespace ET
{
    public struct EntityRef<T>: IEquatable<EntityRef<T>> where T: Entity
    {
        private readonly long instanceId;
        private T entity;

        public override int GetHashCode()
        {
            return this.instanceId.GetHashCode();
        }

        public bool Equals(EntityRef<T> obj)
        {
            return this.instanceId == obj.instanceId;
        }

        public override bool Equals(object obj)
        {
            if (obj is not EntityRef<T> entityRef)
            {
                return false;
            }

            return this.instanceId == entityRef.instanceId;
        }
        
        public static bool operator ==(EntityRef<T> left, EntityRef<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityRef<T> left, EntityRef<T> right)
        {
            return !left.Equals(right);
        }

        private EntityRef(T t)
        {
            if (t == null)
            {
                throw new Exception("cant convert to entityref, entity is null");
            }

            if (t.InstanceId == 0)
            {
                throw new Exception("cant convert to entityref, entity instanceid == 0!");
            }
            this.instanceId = t.InstanceId;
            this.entity = t;
        }
        
        public T Entity
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
            return v.Entity;
        }
    }
    
    
    public struct EntityWeakRef<T>: IEquatable<EntityWeakRef<T>> where T: Entity
    {
        private long instanceId;
        // 使用WeakReference，这样不会导致entity dispose了却无法gc的问题
        // 不过暂时没有测试WeakReference的性能
        private readonly WeakReference<T> weakRef;

        private EntityWeakRef(T t)
        {
            if (t == null)
            {
                throw new Exception("cant convert to entityref, entity is null");
            }

            if (t.InstanceId == 0)
            {
                throw new Exception("cant convert to entityref, entity instanceid == 0!");
            }
            this.instanceId = t.InstanceId;
            this.weakRef = new WeakReference<T>(t);
        }
        
        public T Entity
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
            return v.Entity;
        }
        
        public override int GetHashCode()
        {
            return this.instanceId.GetHashCode();
        }

        public bool Equals(EntityWeakRef<T> obj)
        {
            return this.instanceId == obj.instanceId;
        }

        public override bool Equals(object obj)
        {
            if (obj is not EntityWeakRef<T> entityRef)
            {
                return false;
            }

            return this.instanceId == entityRef.instanceId;
        }
        
        public static bool operator ==(EntityWeakRef<T> left, EntityWeakRef<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityWeakRef<T> left, EntityWeakRef<T> right)
        {
            return !left.Equals(right);
        }
    }
}