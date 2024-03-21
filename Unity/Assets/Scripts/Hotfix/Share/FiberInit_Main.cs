namespace ET
{
    [Invoke((long)SceneType.Main)]
    public class FiberInit_Main: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            //每个纤程都有属于自己的根实体，Scene也是实体，属于某个纤程
            Scene root = fiberInit.Fiber.Root;
            //抛出root场景的入口事件1/2/3，标记了[Event(SceneType.Main)]的类收到事件后会执行对应的方法
            await EventSystem.Instance.PublishAsync(root, new EntryEvent1());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent2());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent3());
        }
    }
}