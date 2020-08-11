using UnityEngine;

namespace ET
{
    public class SceneChangeComponent: Entity
    {
        public AsyncOperation loadMapOperation;
        public ETTaskCompletionSource tcs;
    }
}