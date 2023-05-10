using System;
using System.IO;

namespace ET.Client
{
    [FriendOf(typeof (RoomClientUpdater))]
    public static class RoomClientUpdaterSystem
    {
        public class AwakeSystem: AwakeSystem<RoomClientUpdater>
        {
            protected override void Awake(RoomClientUpdater self)
            {
                Room room = self.GetParent<Room>();
                self.MyId = room.GetParent<Scene>().GetComponent<PlayerComponent>().MyId;
            }
        }
        
        public class UpdateSystem: UpdateSystem<RoomClientUpdater>
        {
            protected override void Update(RoomClientUpdater self)
            {
                self.Update();
            }
        }

        private static void Update(this RoomClientUpdater self)
        {
            Room room = self.GetParent<Room>();
            long timeNow = TimeHelper.ServerFrameTime();
            Scene clientScene = room.GetParent<Scene>();
            
            if (timeNow < room.FixedTimeCounter.FrameTime(room.PredictionFrame + 1))
            {
                return;
            }

            // 最多只预测5帧
            if (room.PredictionFrame - room.RealFrame > 5)
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

        private static OneFrameInputs GetOneFrameMessages(this RoomClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            if (frame <= room.RealFrame)
            {
                return frameBuffer[frame];
            }
            
            // predict
            OneFrameInputs predictionFrame = frameBuffer[frame];
            if (predictionFrame == null)
            {
                throw new Exception($"get frame is null: {frame}, max frame: {frameBuffer.MaxFrame}");
            }
            
            frameBuffer.MoveForward(frame);
            OneFrameInputs realFrame = frameBuffer[room.RealFrame];
            realFrame?.CopyTo(predictionFrame);
            predictionFrame.Inputs[self.MyId] = self.Input;
            
            return predictionFrame;
        }
    }
}