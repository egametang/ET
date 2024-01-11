namespace YIUIFramework
{
    /// <summary>
    /// 释放者接口
    /// </summary>
    public interface IDisposer
    {
        /// <summary>
        /// 表示是否已经释放过了
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// 释放资源，如果执行了这个方法，就不能再使用这个对象了
        /// </summary>
        /// <returns>true表示执行成功，false表示因为某些原因执行失败，最常见的就是，已经执行过这个方法</returns>
        bool Dispose();
    }
}