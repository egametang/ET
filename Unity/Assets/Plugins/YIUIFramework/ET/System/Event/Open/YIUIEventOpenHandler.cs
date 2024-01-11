namespace ET.Client
{
    // 分发UI打开之前监听
    [Event(SceneType.None)]
    public class YIUIEventPanelOpenBeforeHandler: AEvent<Scene, YIUIEventPanelOpenBefore>
    {
        protected override async ETTask Run(Scene scene, YIUIEventPanelOpenBefore arg)
        {
            await YIUIEventComponent.Inst.Run(arg.UIComponentName, arg);
            await scene.Fiber().UIEvent(arg);
        }
    }

    // 分发UI打开之后监听
    [Event(SceneType.None)]
    public class YIUIEventPanelOpenAfterHandler: AEvent<Scene, YIUIEventPanelOpenAfter>
    {
        protected override async ETTask Run(Scene scene, YIUIEventPanelOpenAfter arg)
        {
            await YIUIEventComponent.Inst.Run(arg.UIComponentName, arg);
            await scene.Fiber().UIEvent(arg);
        }
    }
}