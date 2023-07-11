using System;
using System.Collections.Generic;

namespace ET
{
    [UniqueId(-1, 1)]
    public static class LSQueneUpdateIndex
    {
        public const int None = -1;
        public const int LSUpdate = 0;
        public const int Max = 1;
    }
    
    public class LSEntitySystemSingleton: SingletonLock<LSEntitySystemSingleton>, ISingletonAwake
    {
        public TypeSystems TypeSystems { get; private set; }
        
        public void Awake()
        {
            this.TypeSystems = new(LSQueneUpdateIndex.Max);
            foreach (Type type in EventSystem.Instance.GetTypes(typeof (LSEntitySystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                if (obj is not ISystemType iSystemType)
                {
                    continue;
                }

                TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                int index = iSystemType.GetInstanceQueueIndex();
                if (index > LSQueneUpdateIndex.None && index < LSQueneUpdateIndex.Max)
                {
                    oneTypeSystems.QueueFlag[index] = true;
                }
            }
        }
        
        public override void Load()
        {
            World.Instance.AddSingleton<LSEntitySystemSingleton>(true);
        }
        
        public TypeSystems.OneTypeSystems GetOneTypeSystems(Type type)
        {
            return this.TypeSystems.GetOneTypeSystems(type);
        }
        
        public void LSRollback(Entity entity)
        {
            if (entity is not ILSRollback)
            {
                return;
            }
            
            List<object> iLSRollbackSystems = this.TypeSystems.GetSystems(entity.GetType(), typeof (ILSRollbackSystem));
            if (iLSRollbackSystems == null)
            {
                return;
            }

            foreach (ILSRollbackSystem iLSRollbackSystem in iLSRollbackSystems)
            {
                if (iLSRollbackSystem == null)
                {
                    continue;
                }

                try
                {
                    iLSRollbackSystem.Run(entity);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void LSUpdate(LSEntity entity)
        {
            if (entity is not ILSUpdate)
            {
                return;
            }
            
            List<object> iLSUpdateSystems = TypeSystems.GetSystems(entity.GetType(), typeof (ILSUpdateSystem));
            if (iLSUpdateSystems == null)
            {
                return;
            }

            foreach (ILSUpdateSystem iLSUpdateSystem in iLSUpdateSystems)
            {
                try
                {
                    iLSUpdateSystem.Run(entity);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}