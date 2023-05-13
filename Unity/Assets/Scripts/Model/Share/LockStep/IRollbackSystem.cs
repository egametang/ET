using System;
using System.Collections.Generic;

namespace ET
{
    public interface IRollback
    {
    }
    
    public interface IRollbackSystem: ISystemType
    {
        void Run(Entity o);
    }
    
    [LSEntitySystem]
    public abstract class RollbackSystem<T> : IRollbackSystem where T: Entity, IRollback
    {
        void IRollbackSystem.Run(Entity o)
        {
            this.Rollback((T)o);
        }

        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IRollbackSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        protected abstract void Rollback(T self);
    }
}