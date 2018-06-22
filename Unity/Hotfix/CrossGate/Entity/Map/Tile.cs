using UnityEngine;

namespace ETHotfix
{
    public sealed class Tile : Entity
    {
        /// <summary>
        /// 地面层图片组件
        /// </summary>
        public SpriteBaseComponent BaseTileComponent { get; set; }

        /// <summary>
        /// 遮挡层图片组件
        /// </summary>
        public SpriteBaseComponent TopTileComponent { get; set; }

        /// <summary>
        /// 地板层物体
        /// </summary>
        public GameObject BaseTileGameObject { get; set; }

        /// <summary>
        /// 遮挡层物体
        /// </summary>
        public GameObject TopTileGameObject { get; set; }

        /// <summary>
        /// 当前方块的坐标
        /// </summary>
        public int Dong { get; private set; }
        public int Nan { get; private set; }

        /// <summary>
        /// 能否穿越
        /// </summary>
        public bool Walkable { get; private set; }

        public void Awake()
        {
        }

        public void Init(int dong, int nan, TileDat dat)
        {
            //设置位置
            BaseTileGameObject.transform.localPosition = MapHelper.ConverCordToVector3(dong, nan);

            //持有当前坐标
            Dong = dong;
            Nan = nan;

            //生成地板
            if (dat != null && dat.BaseTile > 0)
            {
                ImageConfig config = SqlliteComponent.Instance.GetImageConfig(dat.BaseTile, false);
                if (config != null)
                {
                    Walkable = config.Walkable;
                    BaseTileComponent.Init(config);
                    return;
                }
            }

            Walkable = false;
            BaseTileComponent.Dispose();
            RemoveTop();
        }

        private void RemoveTop()
        {
            if (TopTileGameObject != null)
            {
                GameObjectPoolComponent.Instance.Release(TopTileGameObject, GameObjectType.TopTile);
                TopTileGameObject = null;
                TopTileComponent.Dispose();
                TopTileComponent = null;
            }
        }
        
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();

            if (BaseTileGameObject != null)
            {
                GameObjectPoolComponent.Instance.Release(BaseTileGameObject, GameObjectType.BaseTile);
                BaseTileGameObject = null;
                BaseTileComponent.Dispose();
            }

            RemoveTop();
        }
    }
}