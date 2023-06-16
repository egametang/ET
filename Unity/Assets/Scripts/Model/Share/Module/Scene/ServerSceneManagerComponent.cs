using System;

namespace ET
{
    [ComponentOf(typeof(RootEntity))]
    public class ServerSceneManagerComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static ServerSceneManagerComponent Instance;
    }
}