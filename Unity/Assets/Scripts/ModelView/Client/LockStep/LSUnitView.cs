using UnityEngine;

namespace ET
{
    [ChildOf(typeof(LSUnitViewComponent))]
    public class LSUnitView: Entity, IAwake<GameObject>, IUpdate, ILSRollback
    {
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }
        public EntityRef<LSUnit> Unit;
        public Vector3 Position;
        public Quaternion Rotation;
        public float totalTime;
        public float t;
    }
}