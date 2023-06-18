using System;
using System.Collections.Generic;

namespace ET
{
    public class EntitySystemSingleton: Singleton<EntitySystemSingleton>, ISingletonAwake
    {
        public TypeSystems TypeSystems { get; private set; }
        
        public void Awake()
        {
            this.TypeSystems = new TypeSystems(InstanceQueueIndex.Max);

            foreach (Type type in EventSystem.Instance.GetTypes(typeof (EntitySystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                    int index = iSystemType.GetInstanceQueueIndex();
                    if (index > InstanceQueueIndex.None && index < InstanceQueueIndex.Max)
                    {
                        oneTypeSystems.QueueFlag[index] = true;
                    }
                }
            }
        }
    }
}