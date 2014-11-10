using System;
using Common.Base;

namespace Model
{
    public abstract class DeadComponent: Component<Unit>
    {
        public override Type GetComponentType()
        {
            return typeof (DeadComponent);
        }

        public abstract void Dead();
    }
}
