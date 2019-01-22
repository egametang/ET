using BehaviorDesigner.Runtime.Tasks;
using ETModel;
using System;

namespace ETHotfix
{
    [ObjectSystem]
    public class SeekActionComponentAwakeSystem : AwakeSystem<SeekActionComponent, Entity, HotfixAction, BehaviorTreeConfig>
    {
        public override void Awake(SeekActionComponent self, Entity behaviorTreeParent, HotfixAction hotfixAction, BehaviorTreeConfig behaviorTreeConfig)
        {
            self.Awake(behaviorTreeParent, hotfixAction, behaviorTreeConfig);
        }
    }

    public class SeekActionComponent : Component
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
            Log.Info("追逐到目标");

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