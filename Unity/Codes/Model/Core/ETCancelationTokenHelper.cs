namespace ET
{
    public static class ETCancelationTokenHelper
    {
        public static async ETVoid CancelAfter(this ETCancellationToken self, long afterTimeCancel)
        {
            if (self.IsCancel())
            {
                return;
            }

            await TimerComponent.Instance.WaitAsync(afterTimeCancel);
            
            if (self.IsCancel())
            {
                return;
            }
            
            self.Cancel();
        }
    }
}