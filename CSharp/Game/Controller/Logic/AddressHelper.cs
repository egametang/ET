using Model;

namespace Controller
{
	public static class AddressHelper
	{
		public static string GetAddressByServerName(string serverName)
		{
			ServerInfoConfig serverInfoConfig =
					World.Instance.GetComponent<ConfigComponent>().GetCategory<ServerInfoCategory>()[serverName];
			string address = serverInfoConfig.Host + ":" + serverInfoConfig.Port;
			return address;
		}
	}
}