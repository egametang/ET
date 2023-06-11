using System;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class LocationProxyComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static LocationProxyComponent Instance;
    }
}