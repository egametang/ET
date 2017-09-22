using System.Collections.Generic;

namespace Model
{
    [ObjectEvent]
    public class ClientFrameComponentEvent : ObjectEvent<ClientFrameComponent>, IUpdate
    {
        public void Update()
        {
            this.Get().Update();
        }
    }

    public class ClientFrameComponent: Component
    {
        public int Frame;

        public Queue<FrameMessage> Queue = new Queue<FrameMessage>();

        public int count = 1;

        public void Add(FrameMessage frameMessage)
        {
            this.Queue.Enqueue(frameMessage);
        }

        public void Update()
        {
            int queueCount = this.Queue.Count;
            if (queueCount == 0)
            {
                return;
            }
            this.count = 1 + (queueCount + 3) / 5;
            for (int i = 0; i < this.count; i++)
            {
                this.UpdateFrame();
            }
        }

        private void UpdateFrame()
        {
            FrameMessage frameMessage = this.Queue.Dequeue();
            this.Frame = frameMessage.Frame;

            for (int i = 0; i < frameMessage.Messages.Count; ++i)
            {
	            AFrameMessage message = frameMessage.Messages[i];
	            ushort opcode = Game.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
                Game.Scene.GetComponent<MessageDispatherComponent>().Handle(new MessageInfo() { Opcode= opcode, Message = message });
            }

            Game.Scene.GetComponent<CrossComponent>().Run(CrossIdType.FrameUpdate);
        }
    }
}
