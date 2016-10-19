using Base;

namespace Model
{
	[ObjectEvent]
	public class AppInfoComponentEvent : ObjectEvent<AppInfoComponent>, IAwake<string>
	{
		public void Awake(string appType)
		{
			this.GetValue().Awake(appType);
		}
	}

	public class AppInfoComponent : Component
    {
		private string AppType { get; set; }

		public void Awake(string appType)
		{
			this.AppType = appType;
		}
    }
}