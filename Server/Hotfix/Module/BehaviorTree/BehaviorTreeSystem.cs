using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class BehaviorTreeAwakeSystem : AwakeSystem<BehaviorTree, Behavior>
    {
        public override void Awake(BehaviorTree self, Behavior behavior)
        {
            self.Awake(behavior);
        }
    }

    public static class BehaviorTreeHelper
    {
        public static void Awake(this BehaviorTree self, Behavior behavior)
        {
            if (behavior == null)
            {
                return;
            }

            self.Behavior = behavior;

            self.Behavior.StartWhenEnabled = false;
            self.Behavior.ResetValuesOnRestart = false;

            BindHotfixActions(self);
            BindHotfixComposites(self);
            BindHotfixConditionals(self);
            BindHotfixDecorators(self);

            self.Behavior.EnableBehavior();
        }

        private static void BindHotfixActions(BehaviorTree self)
        {
            var tasks = self.Behavior.FindTasks<HotfixAction>();

            if (tasks == null)
            {
                return;
            }

            foreach (var hotfixAction in tasks)
            {
                var component = BehaviorTreeComponentFactory.Create(self, hotfixAction);

                if (component != null)
                {
                    self.ActionComponents.Add(hotfixAction, component);
                }
            }
        }

        private static void BindHotfixComposites(BehaviorTree self)
        {
            var tasks = self.Behavior.FindTasks<HotfixComposite>();

            if (tasks == null)
            {
                return;
            }

            foreach (var hotfixComposite in tasks)
            {
                var component = BehaviorTreeComponentFactory.Create(self, hotfixComposite);

                if (component != null)
                {
                    self.CompositeComponents.Add(hotfixComposite, component);
                }
            }
        }

        private static void BindHotfixConditionals(BehaviorTree self)
        {
            var tasks = self.Behavior.FindTasks<HotfixConditional>();

            if (tasks == null)
            {
                return;
            }

            foreach (var hotfixConditional in tasks)
            {
                var component = BehaviorTreeComponentFactory.Create(self, hotfixConditional);

                if (component != null)
                {
                    self.ConditionalComponents.Add(hotfixConditional, component);
                }
            }
        }

        private static void BindHotfixDecorators(BehaviorTree self)
        {
            var tasks = self.Behavior.FindTasks<HotfixDecorator>();

            if (tasks == null)
            {
                return;
            }

            foreach (var hotfixDecorator in tasks)
            {
                var component = BehaviorTreeComponentFactory.Create(self, hotfixDecorator);

                if (component != null)
                {
                    self.DecoratorComponents.Add(hotfixDecorator, component);
                }
            }
        }

        public static void EnableBehaior(this BehaviorTree self)
        {
            self?.Behavior?.EnableBehavior();
        }

        public static void DisableBehavior(this BehaviorTree self)
        {
            self?.Behavior?.DisableBehavior();
        }
    }
}
