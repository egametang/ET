using System.IO;
using Base;

namespace Model
{
	[ObjectEvent]
	public class ClientConfigComponentEvent : ObjectEvent<ClientConfigComponent>, IAwake
	{
		public void Awake()
		{
			this.GetValue().Awake();
		}
	}

	public class ClientConfigComponent : Component
    {
		public StartConfig Config { get; private set; }

		public void Awake()
		{
			string s = File.ReadAllText("../Config/StartConfig/ClientConfig.txt");
			this.Config = MongoHelper.FromJson<StartConfig>(s);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Config.Dispose();
		}
    }
}