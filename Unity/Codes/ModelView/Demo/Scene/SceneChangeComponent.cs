using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SceneChangeComponent: Entity, IAwake, IUpdate, IDestroy
    {
        public AsyncOperation loadMapOperation;
        public ETTask tcs;
    }
}