using System;

namespace ETModel
{
    public class FilterEntity: Entity
    {
        public override Component AddComponent(Component component)
        {
            base.AddComponent(component);
            this.Parent.GetParent<FilterComponent>().OnAddComponent(this, component.GetType());
            return component;
        }

        public override K AddComponent<K>()
        {
            Component component = base.AddComponent<K>();
            this.Parent.GetParent<FilterComponent>()?.OnAddComponent(this, component.GetType());
            return (K)component;
        }

        public override K AddComponent<K, P1>(P1 p1)
        {
            Component component = base.AddComponent<K, P1>(p1);
            this.Parent.GetParent<FilterComponent>()?.OnAddComponent(this, component.GetType());
            return (K)component;
        }
        
        public override K AddComponent<K, P1, P2>(P1 p1, P2 p2)
        {
            Component component = base.AddComponent<K, P1, P2>(p1, p2);
            this.Parent.GetParent<FilterComponent>()?.OnAddComponent(this, component.GetType());
            return (K)component;
        }

        public override K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3)
        {
            Component component = base.AddComponent<K, P1, P2, P3>(p1, p2, p3);
            this.Parent.GetParent<FilterComponent>()?.OnAddComponent(this, component.GetType());
            return (K)component;
        }

        public override void RemoveComponent(Type type)
        {
            base.RemoveComponent(type);
            this.Parent.GetParent<FilterComponent>()?.OnRemoveComponent(this, type);
        }

        public override void RemoveComponent<K>()
        {
            base.RemoveComponent<K>();
            this.Parent.GetParent<FilterComponent>()?.OnRemoveComponent(this, typeof(K));
        }
    }
}