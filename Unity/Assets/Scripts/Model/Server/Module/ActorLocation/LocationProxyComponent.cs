using System;

namespace ET.Server
{
    [ComponentOf(typeof(Fiber))]
    public class LocationProxyComponent: SingletonEntity<LocationProxyComponent>, IAwake
    {
    }
}