namespace YIUIFramework
{
    /// <summary>
    /// IDisposer的实现
    /// 可以继承使用
    /// 如果只是用接口，可以直接复制使用
    /// </summary>
    public abstract class Disposer : IDisposer
    {
        public bool Disposed
        {
            get { return m_disposed; }
        }

        public bool Dispose()
        {
            if (m_disposed)
            {
                return false;
            }

            m_disposed = true;
            OnDispose();
            return true;
        }

        /// <summary>
        /// 处理释放相关事情
        /// </summary>
        protected abstract void OnDispose();

        private bool m_disposed;
    }
}