using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class TileClickComponentAwakeSystem : AwakeSystem<TileClickComponent>
    {
        public override void Awake(TileClickComponent self)
        {
            self.Awake();
        }
    }

    public class TileClickComponent : Component
    {
        public void Awake()
        {

        }
    }
}
