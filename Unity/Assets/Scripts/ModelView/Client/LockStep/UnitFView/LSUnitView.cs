using UnityEngine;

namespace ET
{
    [ChildOf(typeof(LSUnitViewComponent))]
    public class LSUnitView: Entity, IAwake<GameObject>, IUpdate
    {
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }
    }
}