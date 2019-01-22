using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
    [ObjectSystem]
    public class BehaviorTreeComponentAwakeSystem : AwakeSystem<BehaviorTreeComponent, BehaviorTree>
    {
        public override void Awake(BehaviorTreeComponent self, BehaviorTree behaviorTree)
        {
            self.Awake(behaviorTree);
        }
    }

    public class BehaviorTreeComponent : Component
    {
        private BehaviorTree behaviorTree;
        private Dictionary<HotfixAction, Component> behaviorTreeActionComponents = new Dictionary<HotfixAction, Component>();
        private Dictionary<HotfixComposite, Component> behaviorTreeCompositeComponents = new Dictionary<HotfixComposite, Component>();
        private Dictionary<HotfixConditional, Component> behaviorTreeConditionalComponents = new Dictionary<HotfixConditional, Component>();
        private Dictionary<HotfixDecorator, Component> behaviorTreeDecoratorComponents = new Dictionary<HotfixDecorator, Component>();

        public void Awake(BehaviorTree behaviorTree)
        {
            if(behaviorTree == null)
            {
                return;
            }

            behaviorTree.StartWhenEnabled = false;
            behaviorTree.ResetValuesOnRestart = false;

            this.behaviorTree = behaviorTree;

            var tasks = behaviorTree.gameObject.Ensure<BehaviorTreeTasks>();

            tasks.Init();

            BindHotfixActions(tasks);
            BindHotfixComposites(tasks);
            BindHotfixConditionals(tasks);
            BindHotfixDecorators(tasks);

            behaviorTree.EnableBehavior();
        }

        private void BindHotfixActions(BehaviorTreeTasks tasks)
        {
            foreach (var hotfixAction in tasks.hotfixActions)
            {
                var component = BehaviorTreeComponentFactory.Create(GetParent<Entity>(), hotfixAction);

                if (component != null)
                {
                    behaviorTreeActionComponents.Add(hotfixAction, component);
                }
            }
        }

        private void BindHotfixComposites(BehaviorTreeTasks tasks)
        {
            foreach (var hotfixComposite in tasks.hotfixComposites)
            {
                var component = BehaviorTreeComponentFactory.Create(GetParent<Entity>(), hotfixComposite);

                if (component != null)
                {
                    behaviorTreeCompositeComponents.Add(hotfixComposite, component);
                }
            }
        }

        private void BindHotfixConditionals(BehaviorTreeTasks tasks)
        {
            foreach (var hotfixConditional in tasks.hotfixConditionals)
            {
                var component = BehaviorTreeComponentFactory.Create(GetParent<Entity>(), hotfixConditional);

                if (component != null)
                {
                    behaviorTreeConditionalComponents.Add(hotfixConditional, component);
                }
            }
        }

        private void BindHotfixDecorators(BehaviorTreeTasks tasks)
        {
            foreach (var hotfixDecorator in tasks.hotfixDecorators)
            {
                var component = BehaviorTreeComponentFactory.Create(GetParent<Entity>(), hotfixDecorator);

                if (component != null)
                {
                    behaviorTreeDecoratorComponents.Add(hotfixDecorator, component);
                }
            }
        }

        public void EnableBehaior()
        {
            behaviorTree?.EnableBehavior();
        }

        public void DisableBehavior()
        {
            behaviorTree?.DisableBehavior();
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach(var item in behaviorTreeActionComponents)
            {
                item.Value.Dispose();
            }

            behaviorTreeActionComponents.Clear();

            foreach (var item in behaviorTreeCompositeComponents)
            {
                item.Value.Dispose();
            }

            behaviorTreeCompositeComponents.Clear();

            foreach (var item in behaviorTreeConditionalComponents)
            {
                item.Value.Dispose();
            }

            behaviorTreeConditionalComponents.Clear();

            foreach (var item in behaviorTreeDecoratorComponents)
            {
                item.Value.Dispose();
            }

            behaviorTreeDecoratorComponents.Clear();

            behaviorTree = null;
        }
    }
}