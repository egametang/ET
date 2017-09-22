namespace Model
{
	[ObjectEvent]
	public class ServerFrameComponentEvent : ObjectEvent<ServerFrameComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	public class ServerFrameComponent: Component
	{
		public int Frame;

		public FrameMessage FrameMessage;
		
		public void Awake()
		{
			this.Frame = 0;
			this.FrameMessage = new FrameMessage() {Frame = this.Frame};

			this.UpdateFrameAsync();
		}

		public async void UpdateFrameAsync()
		{
			TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();

			while (true)
			{
				if (this.Id == 0)
				{
					return;
				}

				await timerComponent.WaitAsync(30);
				
				//MessageHelper.Broadcast(this.FrameMessage);

				++this.Frame;
				this.FrameMessage = new FrameMessage() { Frame = this.Frame };
			}
		}

		public void Add(AFrameMessage message)
		{
			this.FrameMessage.Messages.Add(message);
		}
	}
}
