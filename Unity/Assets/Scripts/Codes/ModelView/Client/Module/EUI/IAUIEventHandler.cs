namespace ET
{
    public interface IAUIEventHandler
    {
        /// <summary>
        /// UI实体加载后,初始化窗口数据
        /// </summary>
        /// <param name="uiBaseWindow"></param>
        void OnInitWindowCoreData(UIBaseWindow uiBaseWindow);
        
        /// <summary>
        /// UI实体加载后，初始化业务逻辑数据
        /// </summary>
        /// <param name="uiBaseWindow"></param>
        void OnInitComponent(UIBaseWindow uiBaseWindow);
        
        /// <summary>
        /// 注册UI业务逻辑事件
        /// </summary>
        /// <param name="uiBaseWindow"></param>
        void OnRegisterUIEvent(UIBaseWindow uiBaseWindow);

        /// <summary>
        /// 打开UI窗口的业务逻辑
        /// </summary>
        /// <param name="uiBaseWindow"></param>
        /// <param name="contextData"></param>
        void OnShowWindow(UIBaseWindow uiBaseWindow, Entity contextData = null);
        
        /// <summary>
        /// 隐藏UI窗口的业务逻辑
        /// </summary>
        /// <param name="uiBaseWindow"></param>
        void OnHideWindow(UIBaseWindow uiBaseWindow);

        /// <summary>
        /// 完全关闭销毁UI窗口之前的业务逻辑，用于完全释放UI相关对象
        /// </summary>
        /// <param name="uiBaseWindow"></param>
        void BeforeUnload(UIBaseWindow uiBaseWindow);
    }
}