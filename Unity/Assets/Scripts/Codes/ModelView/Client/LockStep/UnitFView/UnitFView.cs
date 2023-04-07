using UnityEngine;

namespace ET
{
    [ChildOf(typeof(UnitFViewComponent))]
    public class UnitFView: Entity, IAwake<GameObject>
    {
        public GameObject GameObject;
    }
}