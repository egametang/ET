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
		public ClientConfig Config { get; private set; }

		public void Awake()
		{
			string s = File.ReadAllText("./ClientConfig.txt");
			this.Config = MongoHelper.FromJson<ClientConfig>(s);
		}
    }
}