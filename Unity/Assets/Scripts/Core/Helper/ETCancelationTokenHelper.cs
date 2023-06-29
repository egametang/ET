namespace ET
{
    public static class ETCancelationTokenHelper
    {
        public static async ETTask CancelAfter(this ETCancellationToken self, long afterTimeCancel)
        {
            if (self.IsCancel())
            {
                return;
            }

            await Fiber.Instance.Root.GetComponent<TimerComponent>().WaitAsync(afterTimeCancel);
            
            if (self.IsCancel())
            {
                return;
            }
            
            self.Cancel();
        }
    }
}