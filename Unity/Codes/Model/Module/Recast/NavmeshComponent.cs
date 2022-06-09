using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NavmeshComponent: Entity, IAwake<Func<string, byte[]>>
    {
        public static NavmeshComponent Instance;
        
        // 为了区分客户端跟服务端还有机器人的加载方式，客户端是加载ab包，服务端跟机器人是加载bytes
        public Func<string, byte[]> Loader;
        
        public Dictionary<string, long> Navmeshs = new Dictionary<string, long>();
    }
}