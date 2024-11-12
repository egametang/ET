namespace ET
{
    public interface IYIUIInvokeReturnHandler<out R> : IYIUIInvokeBaseHandler
    {
        R Invoke(Entity self);
    }

    public interface IYIUIInvokeReturnHandler<in P1, out R> : IYIUIInvokeBaseHandler
    {
        R Invoke(Entity self, P1 p1);
    }

    public interface IYIUIInvokeReturnHandler<in P1, in P2, out R> : IYIUIInvokeBaseHandler
    {
        R Invoke(Entity self, P1 p1, P2 p2);
    }

    public interface IYIUIInvokeReturnHandler<in P1, in P2, in P3, out R> : IYIUIInvokeBaseHandler
    {
        R Invoke(Entity self, P1 p1, P2 p2, P3 p3);
    }

    public interface IYIUIInvokeReturnHandler<in P1, in P2, in P3, in P4, out R> : IYIUIInvokeBaseHandler
    {
        R Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public interface IYIUIInvokeReturnHandler<in P1, in P2, in P3, in P4, in P5, out R> : IYIUIInvokeBaseHandler
    {
        R Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }

    public abstract class YIUIInvokeReturnHandler<T, R> : SystemObject, IYIUIInvokeReturnHandler<R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        public R Invoke(Entity self)
        {
            return Invoke((T)self);
        }

        protected abstract R Invoke(T self);
    }

    public abstract class YIUIInvokeReturnHandler<T, P1, R> : SystemObject, IYIUIInvokeReturnHandler<P1, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        public R Invoke(Entity self, P1 p1)
        {
            return Invoke((T)self, p1);
        }

        protected abstract R Invoke(T self, P1 p1);
    }

    public abstract class YIUIInvokeReturnHandler<T, P1, P2, R> : SystemObject, IYIUIInvokeReturnHandler<P1, P2, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        public R Invoke(Entity self, P1 p1, P2 p2)
        {
            return Invoke((T)self, p1, p2);
        }

        protected abstract R Invoke(T self, P1 p1, P2 p2);
    }

    public abstract class YIUIInvokeReturnHandler<T, P1, P2, P3, R> : SystemObject, IYIUIInvokeReturnHandler<P1, P2, P3, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        public R Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            return Invoke((T)self, p1, p2, p3);
        }

        protected abstract R Invoke(T self, P1 p1, P2 p2, P3 p3);
    }

    public abstract class YIUIInvokeReturnHandler<T, P1, P2, P3, P4, R> : SystemObject, IYIUIInvokeReturnHandler<P1, P2, P3, P4, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        public R Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            return Invoke((T)self, p1, p2, p3, p4);
        }

        protected abstract R Invoke(T self, P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public abstract class YIUIInvokeReturnHandler<T, P1, P2, P3, P4, P5, R> : SystemObject, IYIUIInvokeReturnHandler<P1, P2, P3, P4, P5, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        public R Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            return Invoke((T)self, p1, p2, p3, p4, p5);
        }

        protected abstract R Invoke(T self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}