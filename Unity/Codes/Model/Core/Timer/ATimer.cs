namespace ET
{
    public abstract class ATimer<T>: IAction<object> where T: class
    {
        public void Handle(object a)
        {
            this.Run(a as T);
        }

        protected abstract void Run(T t);
    }
}