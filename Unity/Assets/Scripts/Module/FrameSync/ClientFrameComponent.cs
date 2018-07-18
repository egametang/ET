using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace ETModel
{
    public struct SessionFrameMessage
    {
        public Session Session;
        public FrameMessage FrameMessage;
    }
    
	[ObjectSystem]
	public class ClientFrameComponentUpdateSystem : UpdateSystem<ClientFrameComponent>
	{
		public override void Update(ClientFrameComponent self)
		{
			self.Update();
		}
	}


	public class ClientFrameComponent: Component
    {
        public int Frame;

        public Queue<SessionFrameMessage> Queue = new Queue<SessionFrameMessage>();

        public int count = 1;

	    public int waitTime = 100;

        public const int maxWaitTime = 100;

        public void Add(Session session, FrameMessage frameMessage)
        {
            this.Queue.Enqueue(new SessionFrameMessage() {Session = session, FrameMessage = frameMessage});
        }

        public void Update()
        {
			if (this.Queue.Count == 0)
            {
                return;
            }
            SessionFrameMessage sessionFrameMessage = this.Queue.Dequeue();
            this.Frame = sessionFrameMessage.FrameMessage.Frame;
            for (int i = 0; i < sessionFrameMessage.FrameMessage.Message.Count; ++i)
            {
	            OneFrameMessage oneFrameMessage = sessionFrameMessage.FrameMessage.Message[i];

				Session session = sessionFrameMessage.Session;
				OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
	            ushort opcode = (ushort) oneFrameMessage.Op;
	            object instance = opcodeTypeComponent.GetInstance(opcode);

	            byte[] bytes = ByteString.Unsafe.GetBuffer(oneFrameMessage.AMessage);
	            IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(instance, bytes, 0, bytes.Length);
                Game.Scene.GetComponent<MessageDispatherComponent>().Handle(sessionFrameMessage.Session, new MessageInfo((ushort)oneFrameMessage.Op, message));
            }
        }
    }
}
