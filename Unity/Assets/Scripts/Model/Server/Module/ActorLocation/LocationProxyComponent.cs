using System;

namespace ET.Server
{
    [ComponentOf(typeof(VProcess))]
    public class LocationProxyComponent: SingletonEntity<LocationProxyComponent>, IAwake
    {
    }
}