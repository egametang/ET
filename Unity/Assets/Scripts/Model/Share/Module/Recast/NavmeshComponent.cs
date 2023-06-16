using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(RootEntity))]
    public class NavmeshComponent: Entity, IAwake
    {
        [ThreadStatic]
        [StaticField]
        public static NavmeshComponent Instance;
        
        public struct RecastFileLoader
        {
            public string Name { get; set; }
        }
        
        public Dictionary<string, long> Navmeshs = new Dictionary<string, long>();
    }
}