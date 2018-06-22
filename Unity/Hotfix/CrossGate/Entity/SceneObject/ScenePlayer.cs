using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public class SceneObjectAwakeSystem : AwakeSystem<SceneObject>
    {
        public override void Awake(SceneObject self)
        {
            self.Awake();
        }
    }

    /// <summary>
    /// 场景实体玩家信息
    /// </summary>
    public class SceneObject : Entity
    {
        public long UserID { get; set; }
        public GameObject MyGameobject { get; set; }

        public void Awake()
        {
        }

        public void Init(int level, string playername, string title)
        {

        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            UserID = 0;

            if (MyGameobject != null)
            {
                GameObjectPoolComponent.Instance.Release(MyGameobject, GameObjectType.SceneObject);
            }
        }
    }
}
