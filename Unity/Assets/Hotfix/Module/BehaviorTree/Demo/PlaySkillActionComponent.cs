using BehaviorDesigner.Runtime.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class PlaySkillActionComponentAwakeSystem : AwakeSystem<PlaySkillActionComponent, Entity, HotfixAction, BehaviorTreeConfig>
    {
        public override void Awake(PlaySkillActionComponent self, Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            self.Awake(behaviorTreeParent, hotfixAction, behaviorTreeConfig);
        }
    }

    public class PlaySkillActionComponent : Component
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
            Log.Info($"释放一个技能");

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