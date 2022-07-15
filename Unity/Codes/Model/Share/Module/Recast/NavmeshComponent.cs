using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NavmeshComponent: Entity, IAwake
    {
        public static NavmeshComponent Instance;
        
        public Dictionary<string, long> Navmeshs = new Dictionary<string, long>();
    }
}