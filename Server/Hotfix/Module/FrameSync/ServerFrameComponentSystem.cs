using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ServerFrameComponentSystem : AwakeSystem<ServerFrameComponent>
    {
	    public override void Awake(ServerFrameComponent self)
	    {
		    self.Awake();
	    }
    }
	
    public static class ServerFrameComponentEx
    {
        public static void Awake(this ServerFrameComponent self)
        {
            self.Frame = 0;
            self.FrameMessage = new FrameMessage() {Frame = self.Frame};

            self.UpdateFrameAsync();
        }

        public static async void UpdateFrameAsync(this ServerFrameComponent self)
        {
            TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();

            while (true)
            {
                if (self.IsDisposed)
                {
                    return;
                }

                await timerComponent.WaitAsync(100);
				
                MessageHelper.Broadcast(self.FrameMessage);

                ++self.Frame;
                self.FrameMessage = new FrameMessage() { Frame = self.Frame };
            }
        }

        public static void Add(this ServerFrameComponent self, OneFrameMessage oneFrameMessage)
        {
            self.FrameMessage.Messages.Add(oneFrameMessage);
        }
    }
}