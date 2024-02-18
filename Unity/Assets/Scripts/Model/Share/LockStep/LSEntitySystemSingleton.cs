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
    
    [Code]
    public class LSEntitySystemSingleton: Singleton<LSEntitySystemSingleton>, ISingletonAwake
    {
        private TypeSystems TypeSystems { get; set; }
        
        private readonly DoubleMap<Type, long> lsEntityTypeLongHashCode = new();
        
        public void Awake()
        {
            this.TypeSystems = new(LSQueneUpdateIndex.Max);
            foreach (Type type in CodeTypes.Instance.GetTypes(typeof (LSEntitySystemAttribute)))
            {
                SystemObject obj = (SystemObject)Activator.CreateInstance(type);

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
            
            foreach (var kv in CodeTypes.Instance.GetTypes())
            {
                Type type = kv.Value;
                if (typeof(LSEntity).IsAssignableFrom(type))
                {
                    long hash = type.FullName.GetLongHashCode();
                    try
                    {
                        this.lsEntityTypeLongHashCode.Add(type, type.FullName.GetLongHashCode());
                    }
                    catch (Exception e)
                    {
                        Type sameHashType = this.lsEntityTypeLongHashCode.GetKeyByValue(hash);
                        throw new Exception($"long hash add fail: {type.FullName} {sameHashType.FullName}", e);
                    }
                }
            }
        }
        
        public long GetLongHashCode(Type type)
        {
            return this.lsEntityTypeLongHashCode.GetValueByKey(type);
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
            
            List<SystemObject> iLSRollbackSystems = this.TypeSystems.GetSystems(entity.GetType(), typeof (ILSRollbackSystem));
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
            
            List<SystemObject> iLSUpdateSystems = TypeSystems.GetSystems(entity.GetType(), typeof (ILSUpdateSystem));
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