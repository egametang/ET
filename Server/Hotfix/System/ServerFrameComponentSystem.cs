using Model;

namespace Hotfix
{
    [ObjectSystem]
    public class ServerFrameComponentSystem : ObjectSystem<ServerFrameComponent>, IAwake
    {
        public void Awake()
        {
            this.Get().Awake();
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
                if (self.Id == 0)
                {
                    return;
                }

                await timerComponent.WaitAsync(40);
				
                MessageHelper.Broadcast(self.FrameMessage);

                ++self.Frame;
                self.FrameMessage = new FrameMessage() { Frame = self.Frame };
            }
        }

        public static void Add(this ServerFrameComponent self, IFrameMessage message)
        {
            self.FrameMessage.Messages.Add((MessageObject)message);
        }
    }
}