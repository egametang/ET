using System.IO;

namespace Model
{
	[EntityEvent(EntityEventId.ClientConfigComponent)]
	public class ClientConfigComponent: Component
	{
		public StartConfig Config { get; private set; }

		private void Awake()
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