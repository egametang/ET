using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

namespace ETModel
{
    public class BehaviorTree : Entity
    {
        public Behavior Behavior;
        public Dictionary<HotfixAction, Component> ActionComponents = new Dictionary<HotfixAction, Component>();
        public Dictionary<HotfixComposite, Component> CompositeComponents = new Dictionary<HotfixComposite, Component>();
        public Dictionary<HotfixConditional, Component> ConditionalComponents = new Dictionary<HotfixConditional, Component>();
        public Dictionary<HotfixDecorator, Component> DecoratorComponents = new Dictionary<HotfixDecorator, Component>();

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach (var item in ActionComponents)
            {
                item.Value.Dispose();
            }

            ActionComponents.Clear();

            foreach (var item in CompositeComponents)
            {
                item.Value.Dispose();
            }

            CompositeComponents.Clear();

            foreach (var item in ConditionalComponents)
            {
                item.Value.Dispose();
            }

            ConditionalComponents.Clear();

            foreach (var item in DecoratorComponents)
            {
                item.Value.Dispose();
            }

            DecoratorComponents.Clear();

            Behavior = null;
        }
    }
}
