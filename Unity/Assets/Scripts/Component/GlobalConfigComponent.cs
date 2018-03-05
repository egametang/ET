namespace ETModel
{
	[ObjectSystem]
	public class GlobalConfigComponentAwakeSystem : AwakeSystem<GlobalConfigComponent>
	{
		public override void Awake(GlobalConfigComponent t)
		{
			t.Awake();
		}
	}

	public class GlobalConfigComponent : Component
	{
		public static GlobalConfigComponent Instance;
		public GlobalProto GlobalProto;

		public void Awake()
		{
			Instance = this;
			string configStr = ConfigHelper.GetGlobal();
			this.GlobalProto = JsonHelper.FromJson<GlobalProto>(configStr);
		}
	}
}