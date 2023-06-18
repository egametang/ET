using System;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ServerSceneManagerComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static ServerSceneManagerComponent Instance;
    }
}