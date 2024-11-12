namespace ET
{
    public abstract class YIUIInvokeCommonHandler<T, P1, P2, P3, P4, P5, R> :
            IYIUIInvokeHandler,
            IYIUIInvokeHandler<P1>,
            IYIUIInvokeHandler<P1, P2>,
            IYIUIInvokeHandler<P1, P2, P3>,
            IYIUIInvokeHandler<P1, P2, P3, P4>,
            IYIUIInvokeHandler<P1, P2, P3, P4, P5>,
            IYIUIInvokeReturnHandler<R>,
            IYIUIInvokeReturnHandler<P1, R>,
            IYIUIInvokeReturnHandler<P1, P2, R>,
            IYIUIInvokeReturnHandler<P1, P2, P3, R>,
            IYIUIInvokeReturnHandler<P1, P2, P3, P4, R>,
            IYIUIInvokeReturnHandler<P1, P2, P3, P4, P5, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        #region 无返回值

        void IYIUIInvokeHandler.Invoke(Entity self)
        {
            this.InvokeParams(self);
        }

        void IYIUIInvokeHandler<P1>.Invoke(Entity self, P1 p1)
        {
            this.InvokeParams(self, p1);
        }

        void IYIUIInvokeHandler<P1, P2>.Invoke(Entity self, P1 p1, P2 p2)
        {
            this.InvokeParams(self, p1, p2);
        }

        void IYIUIInvokeHandler<P1, P2, P3>.Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            this.InvokeParams(self, p1, p2, p3);
        }

        void IYIUIInvokeHandler<P1, P2, P3, P4>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            this.InvokeParams(self, p1, p2, p3, p4);
        }

        void IYIUIInvokeHandler<P1, P2, P3, P4, P5>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            this.InvokeParams(self, p1, p2, p3, p4, p5);
        }

        protected abstract void InvokeParams(Entity self, params object[] paramVo);

        #endregion

        #region 有返回值

        R IYIUIInvokeReturnHandler<R>.Invoke(Entity self)
        {
            return this.InvokeReturnParams(self);
        }

        R IYIUIInvokeReturnHandler<P1, R>.Invoke(Entity self, P1 p1)
        {
            return this.InvokeReturnParams(self, p1);
        }

        R IYIUIInvokeReturnHandler<P1, P2, R>.Invoke(Entity self, P1 p1, P2 p2)
        {
            return this.InvokeReturnParams(self, p1, p2);
        }

        R IYIUIInvokeReturnHandler<P1, P2, P3, R>.Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            return this.InvokeReturnParams(self, p1, p2, p3);
        }

        R IYIUIInvokeReturnHandler<P1, P2, P3, P4, R>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            return this.InvokeReturnParams(self, p1, p2, p3, p4);
        }

        R IYIUIInvokeReturnHandler<P1, P2, P3, P4, P5, R>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            return this.InvokeReturnParams(self, p1, p2, p3, p4, p5);
        }

        protected abstract R InvokeReturnParams(Entity self, params object[] paramVo);

        #endregion
    }
}