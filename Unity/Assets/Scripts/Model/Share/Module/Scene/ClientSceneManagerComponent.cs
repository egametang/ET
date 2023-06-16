using System;

namespace ET
{
    [ComponentOf(typeof(RootEntity))]
    public class ClientSceneManagerComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static ClientSceneManagerComponent Instance;
    }
}