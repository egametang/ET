using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;

namespace Model
{
	public static class Game
	{
		public static TPoller Poller { get; } = new TPoller();

		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				if (scene != null)
				{
					return scene;
				}
				scene = new Scene();
				scene.AddComponent<EventComponent>();
				scene.AddComponent<TimerComponent>();
				return scene;
			}
		}
		
		public static void Close()
		{
			scene.Dispose();
			scene = null;
		}
	}
}