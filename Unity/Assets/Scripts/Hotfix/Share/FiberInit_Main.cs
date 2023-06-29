namespace ET
{
    [Invoke((long)SceneType.Main)]
    public class FiberInit_Main: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            HandleAsync(fiberInit).Coroutine();
        }

        private async ETTask HandleAsync(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;

            await EventSystem.Instance.PublishAsync(fiber.Root, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(fiber.Root, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(fiber.Root, new EventType.EntryEvent3());
        }
    }
}