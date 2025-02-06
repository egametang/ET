using System;

namespace ET
{
    public interface IClassEvent<T>
    {
    }

    public interface IClassEventSystem
    {
    }
    
    public interface AClassEventSystem<in T>: ISystemType, IClassEventSystem where T: struct
    {
        void Run(Entity e, T t);
    }
    
    public abstract class ClassEventSystem<E, T> : SystemObject, AClassEventSystem<T> where E: Entity, IClassEvent<T> where T: struct
    {
        public void Run(Entity e, T t)
        {
            this.Handle((E)e, t);
        }

        public Type SystemType()
        {
            return typeof(AClassEventSystem<T>);
        }

        public Type Type()
        {
            return typeof(E);
        }

        protected abstract void Handle(Entity e, T t);
    }
}