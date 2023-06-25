using System;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ServerSceneManagerComponent: SingletonEntity<ServerSceneManagerComponent>, IAwake, IDestroy
    {
    }
}