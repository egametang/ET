namespace ETModel
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

		Realm
	}
	
	public sealed class Scene: Entity
	{
		public string Name { get; set; }

		public SceneType SceneType { get; private set; }

		public Scene()
		{
		}

		public Scene(long id): base(id)
		{
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}