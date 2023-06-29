using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Gate)]
    public class FiberInit_Gate: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;

            root.AddComponent<PlayerComponent>();
            root.AddComponent<GateSessionKeyComponent>();

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
        }
    }
}