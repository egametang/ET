namespace Model
{
    [ObjectEvent]
    public class ClientFrameComponentEvent : ObjectEvent<ClientFrameComponent>, IStart
    {
        public void Start()
        {
            this.Get().Start();
        }
    }

    public class ClientFrameComponent: Component
    {
        public int Frame;

        public EQueue<FrameMessage> Queue = new EQueue<FrameMessage>();

        public int count = 1;
        
        public int waitTime;

        public const int maxWaitTime = 40;

        public void Start()
        {
            UpdateAsync();
        }

        public void Add(FrameMessage frameMessage)
        {
            this.Queue.Enqueue(frameMessage);
        }

        public async void UpdateAsync()
        {
            TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
            while (true)
            {
                // 如果队列中消息多于4个，则加速跑帧
                this.waitTime = maxWaitTime;
                if (this.Queue.Count > 4)
                {
                    this.waitTime = maxWaitTime - (this.Queue.Count - 4) * 2;
                }
                // 最快加速一倍
                if (this.waitTime < 20)
                {
                    this.waitTime = 20;
                }

                await timerComponent.WaitAsync(waitTime);

                if (this.Id == 0)
                {
                    return;
                }
                
                this.UpdateFrame();
            }
        }

        private void UpdateFrame()
        {
            if (this.Queue.Count == 0)
            {
                return;
            }
            FrameMessage frameMessage = this.Queue.Dequeue();
            this.Frame = frameMessage.Frame;

            for (int i = 0; i < frameMessage.Messages.Count; ++i)
            {
	            AFrameMessage message = frameMessage.Messages[i];
                Opcode opcode = Game.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
                Game.Scene.GetComponent<MessageDispatherComponent>().Handle(new MessageInfo() { Opcode= opcode, Message = message });
            }
        }
    }
}
