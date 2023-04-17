namespace ET.Server
{
    public static class FrameMessageHelper
    {
        public static void HandleFrameMessage(int fromProcess, long actorId, FrameMessage frameMessage)
        {
            Entity entity = Root.Instance.Get(actorId);
            if (entity == null)
            {
                Log.Error($"actor not found: {actorId} {frameMessage}");
                return;
            }

            Scene scene = entity as Scene;
            BattleScene battleScene = scene.GetComponent<BattleScene>();
            battleScene.FrameBuffer.AddFrameMessage(frameMessage);
        }
    }
}