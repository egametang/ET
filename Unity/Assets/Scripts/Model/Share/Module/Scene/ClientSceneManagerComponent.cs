using System;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ClientSceneManagerComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static ClientSceneManagerComponent Instance;
    }
}