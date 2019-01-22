using BehaviorDesigner.Runtime.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class PatrolActionComponentAwakeSystem : AwakeSystem<PatrolActionComponent, Entity, HotfixAction, BehaviorTreeConfig>
    {
        public override void Awake(PatrolActionComponent self, Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            self.Awake(behaviorTreeParent, hotfixAction, behaviorTreeConfig);
        }
    }

    public class PatrolActionComponent : Component
    {
        private HotfixAction hotfixAction;

        public void Awake(Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            this.hotfixAction = hotfixAction;

            if (this.hotfixAction != null && behaviorTreeConfig != null)
            {
                this.hotfixAction.onTick = OnTick;
            }
        }
        
        private TaskStatus OnTick()
        {
            Patrol();

            return TaskStatus.Running;
        }

        private void Patrol()
        {
            Log.Info($"巡逻中...");
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