using System;
using System.IO;

namespace ET.Client
{
    [FriendOf(typeof (LSClientUpdater))]
    public static class LSClientUpdaterSystem
    {
        public class AwakeSystem: AwakeSystem<LSClientUpdater>
        {
            protected override void Awake(LSClientUpdater self)
            {
                Room room = self.GetParent<Room>();
                self.MyId = room.GetParent<Scene>().GetComponent<PlayerComponent>().MyId;
            }
        }
        
        public class UpdateSystem: UpdateSystem<LSClientUpdater>
        {
            protected override void Update(LSClientUpdater self)
            {
                self.Update();
            }
        }

        private static void Update(this LSClientUpdater self)
        {
            Room room = self.GetParent<Room>();
            long timeNow = TimeHelper.ServerFrameTime();
            Scene clientScene = room.GetParent<Scene>();
            
            if (timeNow < room.FixedTimeCounter.FrameTime(room.PredictionFrame + 1))
            {
                return;
            }

            // 最多只预测5帧
            if (room.PredictionFrame - room.AuthorityFrame > 5)
            {
                return;
            }
            
            ++room.PredictionFrame;
            OneFrameInputs oneFrameInputs = self.GetOneFrameMessages(room.PredictionFrame);
            room.Update(oneFrameInputs, room.PredictionFrame);
            

            FrameMessage frameMessage = NetServices.Instance.FetchMessage<FrameMessage>();
            frameMessage.Frame = room.PredictionFrame;
            frameMessage.Input = self.Input;
            clientScene.GetComponent<SessionComponent>().Session.Send(frameMessage);
        }

        private static OneFrameInputs GetOneFrameMessages(this LSClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            if (frame <= room.AuthorityFrame)
            {
                return frameBuffer.FrameInputs(frame);
            }
            
            // predict
            OneFrameInputs predictionFrame = frameBuffer.FrameInputs(frame);
            if (predictionFrame == null)
            {
                throw new Exception($"get frame is null: {frame}, max frame: {frameBuffer.MaxFrame}");
            }
            
            frameBuffer.MoveForward(frame);
            OneFrameInputs authorityFrame = frameBuffer.FrameInputs(room.AuthorityFrame);
            authorityFrame?.CopyTo(predictionFrame);
            predictionFrame.Inputs[self.MyId] = self.Input;
            
            return predictionFrame;
        }
    }
}