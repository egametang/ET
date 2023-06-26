using System;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ServerSceneManagerComponent: Entity, IAwake, IDestroy
    {
    }
}