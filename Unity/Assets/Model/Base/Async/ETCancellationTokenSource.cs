using System.Threading;

namespace ETModel
{
    [ObjectSystem]
    public class ETCancellationTokenSourceAwakeSystem: AwakeSystem<ETCancellationTokenSource>
    {
        public override void Awake(ETCancellationTokenSource self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();
        }
    }
    
    [ObjectSystem]
    public class ETCancellationTokenSourceAwake2System: AwakeSystem<ETCancellationTokenSource, long>
    {
        public override void Awake(ETCancellationTokenSource self, long afterTimeCancel)
        {
            self.CancellationTokenSource = new CancellationTokenSource();
            self.CancelAfter(afterTimeCancel).Coroutine();
        }
    }
    
    public class ETCancellationTokenSource: Component
    {
        public CancellationTokenSource CancellationTokenSource;

        public void Cancel()
        {
            this.CancellationTokenSource.Cancel();
            this.Dispose();
        }

        public async ETVoid CancelAfter(long afterTimeCancel)
        {
            await Game.Scene.GetComponent<TimerComponent>().WaitAsync(afterTimeCancel);
            this.CancellationTokenSource.Cancel();
            this.Dispose();
        }

        public CancellationToken Token
        {
            get
            {
                return this.CancellationTokenSource.Token;
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.CancellationTokenSource?.Dispose();
            this.CancellationTokenSource = null;
        }
    }
}