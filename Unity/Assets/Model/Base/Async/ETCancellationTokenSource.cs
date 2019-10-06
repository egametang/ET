using System.Collections.Generic;

namespace ETModel
{
    [ObjectSystem]
    public class ETCancellationTokenSourceAwakeSystem: AwakeSystem<ETCancellationTokenSource>
    {
        public override void Awake(ETCancellationTokenSource self)
        {
        }
    }
    
    [ObjectSystem]
    public class ETCancellationTokenSourceAwake2System: AwakeSystem<ETCancellationTokenSource, long>
    {
        public override void Awake(ETCancellationTokenSource self, long afterTimeCancel)
        {
            self.CancelAfter(afterTimeCancel).Coroutine();
        }
    }
    
    [ObjectSystem]
    public class ETCancellationTokenSourceDestroySystem: DestroySystem<ETCancellationTokenSource>
    {
        public override void Destroy(ETCancellationTokenSource self)
        {
            self.cancellationTokens.Clear();
        }
    }
    
    public class ETCancellationTokenSource: Entity
    {
        public readonly List<ETCancellationToken> cancellationTokens = new List<ETCancellationToken>();

        public void Cancel()
        {
            foreach (ETCancellationToken token in this.cancellationTokens)
            {
                token.Cancel();
            }
            
            this.Dispose();
        }

        public async ETVoid CancelAfter(long afterTimeCancel)
        {
            long instanceId = this.InstanceId;
            
            await TimerComponent.Instance.WaitAsync(afterTimeCancel);
            
            if (this.InstanceId != instanceId)
            {
                return;
            }
            this.Dispose();
        }

        public ETCancellationToken Token
        {
            get
            {
                ETCancellationToken etCancellationToken = EntityFactory.Create<ETCancellationToken>(this.Domain);
                this.cancellationTokens.Add(etCancellationToken);

                etCancellationToken.Parent = this;
                return etCancellationToken;
            }
        }
    }
}