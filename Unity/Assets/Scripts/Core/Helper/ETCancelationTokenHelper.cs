namespace ET
{
    public static class ETCancelationTokenHelper
    {
        public static async ETTask CancelAfter(this ETCancellationToken self, Fiber fiber, long afterTimeCancel)
        {
            if (self.IsCancel())
            {
                return;
            }

            await fiber.Root.GetComponent<TimerComponent>().WaitAsync(afterTimeCancel);
            
            if (self.IsCancel())
            {
                return;
            }
            
            self.Cancel();
        }
    }
}