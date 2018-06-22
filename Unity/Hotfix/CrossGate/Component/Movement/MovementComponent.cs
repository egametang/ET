using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class MovementComponentAwakeSystem : AwakeSystem<MovementComponent>
    {
        public override void Awake(MovementComponent self)
        {
            self.Awake();
        }
    }

    public class MovementComponent : Component
    {
        public void Awake()
        {
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

        }
    }
}