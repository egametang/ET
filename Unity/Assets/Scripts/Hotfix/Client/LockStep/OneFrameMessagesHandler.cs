using System;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class OneFrameMessagesHandler: AMHandler<OneFrameMessages>
    {
        protected override async ETTask Run(Session session, OneFrameMessages message)
        {
            Room room = session.DomainScene().GetComponent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            if (message.Frame != room.RealFrame + 1)
            {
                throw new Exception($"recv oneframeMessage frame error: {message.Frame} {room.RealFrame}");
            }

            ++room.RealFrame;
            // 服务端返回的消息比预测的还早
            if (room.RealFrame > room.PredictionFrame)
            {
                OneFrameMessages realFrame = frameBuffer[room.RealFrame];
                message.CopyTo(realFrame);
                return;
            }
            
            // 服务端返回来的消息，跟预测消息对比
            OneFrameMessages predictionMessage = frameBuffer[message.Frame];
            // 对比失败有两种可能，
            // 1是别人的输入预测失败，这种很正常，
            // 2 自己的输入对比失败，这种情况是自己发送的消息比服务器晚到了，服务器使用了你的上一次输入
            // 回滚重新预测的时候，自己的输入不用变化
            if (message != predictionMessage)
            {
                message.CopyTo(predictionMessage);
                // 回滚到frameBuffer.RealFrame
                LSHelper.Rollback(room, room.RealFrame);
            }

            // 回收消息，减少GC
            NetServices.Instance.RecycleMessage(message);
            await ETTask.CompletedTask;
        }
    }
}