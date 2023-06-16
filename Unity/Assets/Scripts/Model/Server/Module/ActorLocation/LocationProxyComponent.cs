using System;

namespace ET.Server
{
    [ComponentOf(typeof(RootEntity))]
    public class LocationProxyComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static LocationProxyComponent Instance;
    }
}