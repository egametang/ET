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
            
            if (message.Frame != frameBuffer.RealFrame + 1)
            {
                throw new Exception($"recv oneframeMessage frame error: {message.Frame} {frameBuffer.RealFrame}");
            }

            // 服务端返回来的消息，跟预测消息对比
            OneFrameMessages predictionMessage = frameBuffer.GetFrame(message.Frame);
            
            ++frameBuffer.RealFrame;
            if (message != predictionMessage)
            {
                message.CopyTo(predictionMessage);
                // 回滚到frameBuffer.RealFrame
                LSHelper.Rollback(room, frameBuffer.RealFrame);
            }

            await ETTask.CompletedTask;
        }
    }
}