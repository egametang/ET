namespace ET
{
    public struct FixUpdateInvokerArgs
    {
        public Entity Entity;
    }
    
    public abstract class AFixUpdateHandler<T>: AInvokeHandler<FixUpdateInvokerArgs> where T: Entity
    {
        public override void Handle(FixUpdateInvokerArgs args)
        {
            this.Run(args.Entity as T);
        }

        protected abstract void Run(T t);
    }
}