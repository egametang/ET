using System.Collections.Generic;
using System.Linq;

namespace Base
{
	public enum SceneType
	{
		Share,
		Game,
		Login,
		Lobby,
		Map,
		Launcher,
		Robot,
        BehaviorTreeScene,
		RobotClient,
	}

	public sealed class Scene: Entity<Scene>
	{
		public SceneType SceneType { get; }

		public Scene(string name, SceneType sceneType): base(name)
		{
			this.SceneType = sceneType;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}