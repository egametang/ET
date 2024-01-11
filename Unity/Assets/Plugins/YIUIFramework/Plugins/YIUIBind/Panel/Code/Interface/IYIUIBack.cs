namespace YIUIFramework
{
    /// <summary>
    /// 触发堆栈时 会有消息
    /// 参数info 是给你看触发你消息的是谁
    /// 不要滥用 不要修改里面的值
    /// </summary>
    public interface IYIUIBack
    {
        /// <summary>
        /// 是被关闭触发 (有界面打开 当前界面被关闭)
        /// 自己被关闭
        /// </summary>
        /// <param name="info">触发的那个界面是谁</param>
        void DoBackClose(PanelInfo info);

        /// <summary>
        /// 是添加触发 (有其他界面关闭 当前界面被打开)
        /// 自己被打开
        /// </summary>
        /// <param name="info">触发的那个界面是谁</param>
        void DoBackAdd(PanelInfo info);

        /// <summary>
        /// Home触发 (有其他界面打开 当前界面被关闭)
        /// 自己被关闭
        /// </summary>
        /// <param name="info">触发的那个界面是谁</param>
        void DoBackHome(PanelInfo info);
    }
}