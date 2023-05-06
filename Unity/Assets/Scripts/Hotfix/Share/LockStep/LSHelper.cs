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
            OneFrameMessages realFrameMessage = frameBuffer.GetFrame(frame);
            // 执行RealFrame
            room.Update(realFrameMessage, frame);

            
            // 重新执行预测的帧
            for (int i = frameBuffer.RealFrame + 1; i <= frameBuffer.PredictionFrame; ++i)
            {
                OneFrameMessages oneFrameMessages = frameBuffer.GetFrame(i);
                CopyOtherInputsTo(room, realFrameMessage, oneFrameMessages); // 重新预测剩下预测过的消息
                room.Update(oneFrameMessages, i);
            }
            
            RollbackHelper.Rollback(room);
            
            Log.Debug($"roll back finish {frame}");
        }

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