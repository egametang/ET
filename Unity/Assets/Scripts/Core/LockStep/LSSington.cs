using System;

namespace ET
{
    public class LSSington: Singleton<LSSington>, ISingletonAwake
    {
        public TypeSystems TypeSystems = new TypeSystems(LSQueneUpdateIndex.Max);
        
        public void Awake()
        {
            foreach (Type type in EventSystem.Instance.GetTypes(typeof (LSSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                }
            }
        }
    }
}