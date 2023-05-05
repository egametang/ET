namespace ET
{
    public static class LSHelper
    {
        // 回滚
        public static void Rollback(Room self, int frame)
        {
            Log.Debug($"Room Scene roll back to {frame}");
            self.LSWorld.Dispose();
            FrameBuffer frameBuffer = self.FrameBuffer;
            
            // 回滚
            self.LSWorld = frameBuffer.GetLSWorld(frame);

            // 从回滚的地方重新执行预测的帧
            for (int i = frameBuffer.RealFrame + 1; i < frameBuffer.PredictionFrame; ++i)
            {
                OneFrameMessages oneFrameMessages = frameBuffer.GetFrame(i);
                self.Update(oneFrameMessages);
            }
            
            RollbackHelper.Rollback(self);
        }
    }
}