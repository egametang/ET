namespace ET
{
    public interface ITimer
    {
        void Handle(object args);
    }
    
    public abstract class ATimer<T>: ITimer where T: class
    {
        public void Handle(object args)
        {
            this.Run(args as T);
        }

        public abstract void Run(T t);
    }
}