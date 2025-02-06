using System;
using System.Collections.Generic;

namespace ET
{
    public class TypeSystems
    {
        public class OneTypeSystems
        {
            public readonly UnOrderMultiMap<Type, SystemObject> Map = new();
            // 这里不用hash，数量比较少，直接for循环速度更快
            public readonly List<Type> ClassType = new();
        }
        
        private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

        public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            if (systems != null)
            {
                return systems;
            }

            systems = new OneTypeSystems();
            this.typeSystemsMap.Add(type, systems);
            return systems;
        }

        public OneTypeSystems GetOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            return systems;
        }

        public List<SystemObject> GetSystems(Type type, Type systemType)
        {
            OneTypeSystems oneTypeSystems = null;
            if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
            {
                return null;
            }

            if (!oneTypeSystems.Map.TryGetValue(systemType, out List<SystemObject> systems))
            {
                return null;
            }

            return systems;
        }
    }
}