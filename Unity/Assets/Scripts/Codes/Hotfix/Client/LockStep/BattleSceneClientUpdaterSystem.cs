using System;
using MongoDB.Bson;

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
            
            OneFrameMessages oneFrameMessages = GetOneFrameMessages(self, frameBuffer.NowFrame);
            battleScene.Update(oneFrameMessages);
            ++frameBuffer.NowFrame;
        }

        private static OneFrameMessages GetOneFrameMessages(this BattleSceneClientUpdater self, int frame)
        {
            BattleScene battleScene = self.GetParent<BattleScene>();
            FrameBuffer frameBuffer = battleScene.FrameBuffer;
            
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
            OneFrameMessages predictionFrame  = preFrame != null? MongoHelper.Clone(preFrame) : new OneFrameMessages();
            predictionFrame.Frame = frame;

            PlayerComponent playerComponent = battleScene.GetParent<Scene>().GetComponent<PlayerComponent>();
            long myId = playerComponent.MyId;

            FrameMessage frameMessage = new() { InputInfo = new LSInputInfo(), Frame = frame };
            frameMessage.InputInfo.V = self.InputInfo.V;
            frameMessage.InputInfo.Button = self.InputInfo.Button;

            predictionFrame.InputInfos[myId] = frameMessage.InputInfo;
            
            
            self.Parent.GetParent<Scene>().GetComponent<SessionComponent>().Session.Send(frameMessage);
            
            
            return predictionFrame;
        }
    }
}