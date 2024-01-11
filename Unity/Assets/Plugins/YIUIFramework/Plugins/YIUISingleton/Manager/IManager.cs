namespace YIUIFramework
{
    public interface IManager : ISingleton
    {
        /// <summary>
        /// 已经初始化结束 且成功了
        /// 失败了可以重复初始化
        /// </summary>
        bool InitedSucceed { get; }
        
        //激活状态
        //激活被关闭时 Update,LateUpdate,FixedUpdate 都会被停止
        //其他不影响
        bool Enabled { get; }
    }
}