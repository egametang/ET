using BehaviorDesigner.Runtime.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class LogActionComponentAwakeSystem : AwakeSystem<LogActionComponent, Entity, HotfixAction, BehaviorTreeConfig>
    {
        public override void Awake(LogActionComponent self, Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            self.Awake(behaviorTreeParent, hotfixAction, behaviorTreeConfig);
        }
    }

    public class LogActionComponent : Component
    {
        private HotfixAction hotfixAction;

        public void Awake(Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            this.hotfixAction = hotfixAction;

            if (this.hotfixAction != null)
            {
                this.hotfixAction.onTick = OnTick;
            }
        }

        private TaskStatus OnTick()
        {
            Log.Error("Hello HotfixAction");
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
        }
    }
}