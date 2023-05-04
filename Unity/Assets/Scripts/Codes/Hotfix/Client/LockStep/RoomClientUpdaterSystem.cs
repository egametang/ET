using System;
using System.IO;
using MongoDB.Bson;

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
        
        [FriendOf(typeof (Room))]
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
            FrameBuffer frameBuffer = room.FrameBuffer;
            long timeNow = TimeHelper.ServerFrameTime();
            Scene clientScene = room.GetParent<Scene>();
            if (!room.FixedTimeCounter.IsTimeout(timeNow, frameBuffer.NowFrame))
            {
                return;
            }
            
            OneFrameMessages oneFrameMessages = GetOneFrameMessages(self, frameBuffer.NowFrame);
            room.Update(oneFrameMessages);
            ++frameBuffer.NowFrame;

            FrameMessage frameMessage = NetServices.Instance.FetchMessage<FrameMessage>();
            frameMessage.Frame = oneFrameMessages.Frame;
            frameMessage.Input = self.Input;
            clientScene.GetComponent<SessionComponent>().Session.Send(frameMessage);
        }

        private static OneFrameMessages GetOneFrameMessages(this RoomClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            if (frame <= frameBuffer.RealFrame)
            {
                return frameBuffer.GetFrame(frame);
            }
            
            // predict
            return GetPredictionOneFrameMessage(self, frame);
        }

        // 获取预测一帧的消息
        private static OneFrameMessages GetPredictionOneFrameMessage(this RoomClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            OneFrameMessages preFrame = room.FrameBuffer.GetFrame(frame - 1);
            OneFrameMessages predictionFrame  = preFrame != null? MongoHelper.Clone(preFrame) : new OneFrameMessages();
            predictionFrame.Frame = frame;

            predictionFrame.Inputs[self.MyId] = new() {V = self.Input.V, Button = self.Input.Button};
            
            room.FrameBuffer.AddFrame(predictionFrame);
            
            return predictionFrame;
        }
        
        // 获取预测一帧的消息
        private static OneFrameMessages GetRePredictionOneFrameMessage(this RoomClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            OneFrameMessages preFrame = frameBuffer.GetFrame(frame - 1);
            OneFrameMessages predictionFrame = frameBuffer.GetFrame(frame);
            foreach (var kv in predictionFrame.Inputs)
            {
                if (kv.Key == self.MyId)
                {
                    continue;
                }

                LSInput preInput = preFrame.Inputs[kv.Key];
                predictionFrame.Inputs[kv.Key] = preInput;
            }

            LSInput oldMyInput = frameBuffer.GetFrame(predictionFrame.Frame).Inputs[self.MyId];

            predictionFrame.Inputs[self.MyId] = oldMyInput;
            
            predictionFrame.Frame = frame;
            return predictionFrame;
        }
    }
}