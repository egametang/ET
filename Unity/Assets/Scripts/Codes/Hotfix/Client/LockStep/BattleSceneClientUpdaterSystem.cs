using System;

namespace ET.Client
{
    [FriendOf(typeof (BattleSceneClientUpdater))]
    public static class BattleSceneClientUpdaterSystem
    {
        [FriendOf(typeof (BattleScene))]
        public class UpdateSystem: UpdateSystem<BattleSceneClientUpdater>
        {
            protected override void Update(BattleSceneClientUpdater self)
            {
                self.Update();
            }
        }

        private static void Update(this BattleSceneClientUpdater self)
        {
            BattleScene battleScene = self.GetParent<BattleScene>();

            FrameBuffer frameBuffer = battleScene.FrameBuffer;

            long timeNow = TimeHelper.ServerFrameTime();
            if (timeNow < battleScene.StartTime + battleScene.FrameBuffer.NowFrame * LSConstValue.UpdateInterval)
            {
                return;
            }

            // 执行到PredictionFrame, 每次update最多执行5帧
            if (frameBuffer.NowFrame < frameBuffer.RealFrame + frameBuffer.PredictionCount)
            {
                int j = 0;
                for (int i = frameBuffer.NowFrame; i < frameBuffer.RealFrame + frameBuffer.PredictionCount; ++i)
                {
                    if (++j % 5 == 0)
                    {
                        break;
                    }

                    ++frameBuffer.NowFrame;
                    OneFrameMessages oneFrameMessages = GetOneFrameMessages(self, i);
                    battleScene.Update(oneFrameMessages);
                }
            }
        }

        private static OneFrameMessages GetOneFrameMessages(this BattleSceneClientUpdater self, int frame)
        {
            BattleScene battleScene = self.GetParent<BattleScene>();
            FrameBuffer frameBuffer = battleScene.FrameBuffer;
            if (frame != frameBuffer.NowFrame + 1)
            {
                throw new Exception($"get frame error: {frame} {frameBuffer.NowFrame} {frameBuffer.RealFrame}");
            }

            if (frame <= frameBuffer.RealFrame)
            {
                return frameBuffer.GetFrame(frame);
            }

            // predict
            return GetPredictionOneFrameMessage(self, frame);
        }

        // 获取预测一帧的消息
        private static OneFrameMessages GetPredictionOneFrameMessage(this BattleSceneClientUpdater self, int frame)
        {
            BattleScene battleScene = self.GetParent<BattleScene>();
            OneFrameMessages preFrame = battleScene.FrameBuffer.GetFrame(frame - 1);
            if (preFrame == null)
            {
                return null;
            }

            OneFrameMessages predictionFrame = MongoHelper.Clone(preFrame);

            predictionFrame.Frame = frame;

            PlayerComponent playerComponent = battleScene.GetParent<Scene>().GetComponent<PlayerComponent>();
            long myId = playerComponent.MyId;
            predictionFrame.InputInfos[myId] = self.InputInfo;
            return predictionFrame;
        }
    }
}