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

	public sealed class Scene: Entity
	{
		public Scene Owner { get; set; }

		public string Name { get; set; }

		public SceneType SceneType { get; private set; }

		public Scene(string name, SceneType sceneType)
		{
			this.Name = name;
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