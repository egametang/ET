using UnityEngine;

namespace ET.Client
{
    public class SceneChangeComponent: Entity, IAwake, IUpdate, IDestroy
    {
        public AsyncOperation loadMapOperation;
        public ETTask tcs;
    }
}