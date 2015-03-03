namespace Model
{
	public class ServerInfoConfig : AConfig
	{
		public string Zone { get; set; }
		public string Name { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public ServerType ServerType { get; set; }
	}
}
