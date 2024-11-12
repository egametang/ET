namespace ET
{
    // 分发数值监听
    // 服务端需要分发, 客户端也要分发 所以是所有
    [Event(SceneType.All)]
    public partial class NumericChangeEvent_NotifyHandler : AEvent<Scene, NumericChange>
    {
        protected override async ETTask Run(Scene scene, NumericChange args)
        {
            //先触发单条监听 再触发所有监听
            await NumericHandlerComponent.Instance.Run(args);

            //最后触发动态消息 这个消息会直接传递到对应类中 可做刷新等操作 具体看案例
            await NumericHandlerDynamicComponent.Instance.Run(args);
        }
    }
}