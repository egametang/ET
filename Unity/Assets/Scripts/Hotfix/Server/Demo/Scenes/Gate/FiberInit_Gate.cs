using System.Net;

namespace ET.Server
{
    [Invoke((int)SceneType.Gate)]
    public class FiberInit_Gate: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;

            fiber.AddComponent<PlayerComponent>();
            fiber.AddComponent<GateSessionKeyComponent>();

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)fiber.Id);
            fiber.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
        }
    }
}