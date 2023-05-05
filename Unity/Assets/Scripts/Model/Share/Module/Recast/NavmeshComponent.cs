using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NavmeshComponent: Entity, IAwake
    {
        [StaticField]
        public static NavmeshComponent Instance;
        
        public struct RecastFileLoader
        {
            public string Name { get; set; }
        }
        
        public Dictionary<string, long> Navmeshs = new Dictionary<string, long>();
    }
}