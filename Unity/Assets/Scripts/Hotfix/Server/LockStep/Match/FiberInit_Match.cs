using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Match)]
    public class FiberInit_Match: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;

            root.AddComponent<MatchComponent>();
        }
    }
}