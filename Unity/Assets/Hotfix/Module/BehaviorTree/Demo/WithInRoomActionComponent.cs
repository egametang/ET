using BehaviorDesigner.Runtime.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class WithInRoomActionComponentAwakeSystem : AwakeSystem<WithInRoomActionComponent, Entity, HotfixAction, BehaviorTreeConfig>
    {
        public override void Awake(WithInRoomActionComponent self, Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            self.Awake(behaviorTreeParent, hotfixAction, behaviorTreeConfig);
        }
    }

    public class WithInRoomActionComponent : Component
    {
        private HotfixAction hotfixAction;
        private Entity behaviorTreeParent;

        public void Awake(Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            this.hotfixAction = hotfixAction;
            this.behaviorTreeParent = behaviorTreeParent;
            
            if (this.hotfixAction != null)
            {
                this.hotfixAction.onTick = OnTick;
            }
        }

        private TaskStatus OnTick()
        {
            Log.Info($"检查到目标");

            return TaskStatus.Success;
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();

            if (hotfixAction != null)
            {
                hotfixAction.onTick = null;
            }

            hotfixAction = null;
            behaviorTreeParent = null;
        }
    }
}
