namespace Hotfix
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

		public Model.Scene ModelScene { get; set; } = new Model.Scene();

		public string Name { get; set; }

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

			this.ModelScene.Dispose();
		}
	}
}