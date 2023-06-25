using System;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ClientSceneManagerComponent: SingletonEntity<ClientSceneManagerComponent>, IAwake
    {
    }
}