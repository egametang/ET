using ET.Client;

namespace ET
{
    public static class LSHelper
    {
        // 回滚
        public static void Rollback(Room room, int frame)
        {
            Log.Debug($"roll back start {frame}");
            room.LSWorld.Dispose();
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            // 回滚
            room.LSWorld = frameBuffer.GetLSWorld(frame);
            OneFrameMessages realFrameMessage = frameBuffer[frame];
            // 执行RealFrame
            room.Update(realFrameMessage, frame);

            
            // 重新执行预测的帧
            for (int i = room.RealFrame + 1; i <= room.PredictionFrame; ++i)
            {
                OneFrameMessages oneFrameMessages = frameBuffer[i];
                CopyOtherInputsTo(room, realFrameMessage, oneFrameMessages); // 重新预测消息
                room.Update(oneFrameMessages, i);
            }
            
            RollbackHelper.Rollback(room);
            
            Log.Debug($"roll back finish {frame}");
        }

        // 重新调整预测消息，只需要调整其他玩家的输入
        private static void CopyOtherInputsTo(Room room, OneFrameMessages from, OneFrameMessages to)
        {
            long myId = room.GetComponent<RoomClientUpdater>().MyId;
            foreach (var kv in from.Inputs)
            {
                if (kv.Key == myId)
                {
                    continue;
                }
                to.Inputs[kv.Key] = kv.Value;
            }
        }
    }
}