using System;
using System.Collections.Generic;

namespace ETModel
{
    public struct SessionFrameMessage
    {
        public Session Session;
        public FrameMessage FrameMessage;
    }
    
    [ObjectSystem]
    public class ClientFrameComponentStartSystem : StartSystem<ClientFrameComponent>
    {
	    public override void Start(ClientFrameComponent t)
	    {
		    t.Start();
	    }
    }
	public class ClientFrameComponent: Component
    {
        public int Frame;

        public Queue<SessionFrameMessage> Queue = new Queue<SessionFrameMessage>();

        public int count = 1;
        
        public int waitTime;

        public const int maxWaitTime = 100;

        public void Start()
        {
            UpdateAsync();
        }

        public void Add(Session session, FrameMessage frameMessage)
        {
            this.Queue.Enqueue(new SessionFrameMessage() {Session = session, FrameMessage = frameMessage});
        }

        public async void UpdateAsync()
        {
	        try
	        {
		        TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
		        while (true)
		        {
			        await timerComponent.WaitAsync(waitTime);

			        if (this.IsDisposed)
			        {
				        return;
			        }

			        this.UpdateFrame();
		        }
			}
	        catch (Exception e)
	        {
		        Log.Error(e);
	        }
        }

        private void UpdateFrame()
        {
            if (this.Queue.Count == 0)
            {
                return;
            }
            SessionFrameMessage sessionFrameMessage = this.Queue.Dequeue();
            this.Frame = sessionFrameMessage.FrameMessage.Frame;

            for (int i = 0; i < sessionFrameMessage.FrameMessage.Messages.Count; ++i)
            {
	            OneFrameMessage oneFrameMessage = sessionFrameMessage.FrameMessage.Messages[i];

				Session session = sessionFrameMessage.Session;
				OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
	            Type type = opcodeTypeComponent.GetType(oneFrameMessage.Op);

	            IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(type, oneFrameMessage.AMessage);
                Game.Scene.GetComponent<MessageDispatherComponent>().Handle(sessionFrameMessage.Session, new MessageInfo(oneFrameMessage.Op, message));
            }
        }
    }
}
