using System;
using System.Collections.Generic;

namespace ET
{
    public class TypeSystems
    {
        public class OneTypeSystems
        {
            public OneTypeSystems(int count)
            {
                this.QueueFlag = new bool[count];
            }
            
            public readonly UnOrderMultiMap<Type, object> Map = new();
            // 这里不用hash，数量比较少，直接for循环速度更快
            public readonly bool[] QueueFlag;
        }

        private readonly int count;

        public TypeSystems(int count)
        {
            this.count = count;
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

            systems = new OneTypeSystems(this.count);
            this.typeSystemsMap.Add(type, systems);
            return systems;
        }

        public OneTypeSystems GetOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            return systems;
        }

        public List<object> GetSystems(Type type, Type systemType)
        {
            OneTypeSystems oneTypeSystems = null;
            if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
            {
                return null;
            }

            if (!oneTypeSystems.Map.TryGetValue(systemType, out List<object> systems))
            {
                return null;
            }

            return systems;
        }
    }
}