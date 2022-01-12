using UnityEngine;

namespace ET
{
    public class SceneChangeComponent: Entity, IAwake, IUpdate, IDestroy
    {
        public AsyncOperation loadMapOperation;
        public ETTask tcs;
    }
}