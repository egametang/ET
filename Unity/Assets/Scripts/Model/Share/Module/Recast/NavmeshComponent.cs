using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class NavmeshComponent: SingletonEntity<NavmeshComponent>, IAwake
    {
        public struct RecastFileLoader
        {
            public string Name { get; set; }
        }
        
        public Dictionary<string, long> Navmeshs = new Dictionary<string, long>();
    }
}