namespace ET
{
    public interface IYIUIInvokeBaseHandler
    {
        string InvokeType { get; set; }
    }

    public interface IYIUIInvokeHandler : IYIUIInvokeBaseHandler
    {
        void Invoke(Entity self);
    }

    public interface IYIUIInvokeHandler<in P1> : IYIUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1);
    }

    public interface IYIUIInvokeHandler<in P1, in P2> : IYIUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2);
    }

    public interface IYIUIInvokeHandler<in P1, in P2, in P3> : IYIUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2, P3 p3);
    }

    public interface IYIUIInvokeHandler<in P1, in P2, in P3, in P4> : IYIUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public interface IYIUIInvokeHandler<in P1, in P2, in P3, in P4, in P5> : IYIUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }

    public abstract class YIUIInvokeHandler<T> : SystemObject, IYIUIInvokeHandler where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self)
        {
            Invoke((T)self);
        }

        protected abstract void Invoke(T self);
    }

    public abstract class YIUIInvokeHandler<T, P1> : SystemObject, IYIUIInvokeHandler<P1> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1)
        {
            Invoke((T)self, p1);
        }

        protected abstract void Invoke(T self, P1 p1);
    }

    public abstract class YIUIInvokeHandler<T, P1, P2> : SystemObject, IYIUIInvokeHandler<P1, P2> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2)
        {
            Invoke((T)self, p1, p2);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2);
    }

    public abstract class YIUIInvokeHandler<T, P1, P2, P3> : SystemObject, IYIUIInvokeHandler<P1, P2, P3> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            Invoke((T)self, p1, p2, p3);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2, P3 p3);
    }

    public abstract class YIUIInvokeHandler<T, P1, P2, P3, P4> : SystemObject, IYIUIInvokeHandler<P1, P2, P3, P4> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            Invoke((T)self, p1, p2, p3, p4);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public abstract class YIUIInvokeHandler<T, P1, P2, P3, P4, P5> : SystemObject, IYIUIInvokeHandler<P1, P2, P3, P4, P5> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            Invoke((T)self, p1, p2, p3, p4, p5);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}