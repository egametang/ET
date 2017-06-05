namespace Model
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
		public Scene Parent { get; set; }

		public string Name { get; set; }

		public SceneType SceneType { get; private set; }

		public Scene(): base(EntityType.Scene)
		{
		}

		public Scene(long id): base(id, EntityType.Scene)
		{
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