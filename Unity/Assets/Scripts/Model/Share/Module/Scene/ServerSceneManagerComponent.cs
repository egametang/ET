using System;

namespace ET
{
    [ComponentOf(typeof(VProcess))]
    public class ServerSceneManagerComponent: SingletonEntity<ServerSceneManagerComponent>, IAwake, IDestroy
    {
    }
}