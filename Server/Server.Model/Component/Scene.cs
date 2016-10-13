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

		Realm,
		Gate,
	}

	[ObjectEvent]
	public class SceneEvent : ObjectEvent<Scene>, IAwake<SceneType, string>
	{
		public void Awake(SceneType sceneType, string name)
		{
			this.GetValue().Awake(sceneType, name);
		}
	}

	public sealed class Scene: Component
	{
		public SceneType SceneType { get; set; }
		public string Name { get; set; }

		public void Awake(SceneType sceneType, string name)
		{
			this.SceneType = sceneType;
			this.Name = name;
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