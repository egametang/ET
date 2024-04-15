namespace ET
{
    public abstract class ATimer<T>: AInvokeHandler<TimerCallback> where T: class
    {
        public override void Handle(TimerCallback a)
        {
            this.Run(a.Args as T);
        }

        protected abstract void Run(T t);
    }
}