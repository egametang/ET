namespace Model
{
	[ObjectEvent]
	public class GlobalConfigComponentEvent : ObjectEvent<GlobalConfigComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
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
			this.GlobalProto = MongoHelper.FromJson<GlobalProto>(configStr);
		}
	}
}