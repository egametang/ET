using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public class RaycastComponentAwakeSystem : AwakeSystem<RaycastComponent>
    {
        public override void Awake(RaycastComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class RaycastComponentLateUpdateSystem : LateUpdateSystem<RaycastComponent>
    {
        public override void LateUpdate(RaycastComponent self)
        {
            self.LateUpdate();
        }
    }

    public class RaycastComponent : Component
    {
        private int mapMask;

        public void Awake()
        {
            this.mapMask = LayerMask.GetMask("Map");
        }

        public void LateUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), this.mapMask);
                if (collider != null)
                {
                    Log.Debug(collider.gameObject.name + " / " + collider.gameObject.transform.position);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            mapMask = 0;
        }
    }
}
