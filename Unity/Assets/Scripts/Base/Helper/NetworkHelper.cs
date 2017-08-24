using System.Net;

namespace Model
{
	public static class NetworkHelper
	{
		public static uint NetworkToHostOrder(uint a)
		{
			return (uint) IPAddress.NetworkToHostOrder((int) a);
		}

		public static ushort NetworkToHostOrder(ushort a)
		{
			return (ushort)IPAddress.NetworkToHostOrder((short)a);
		}

		public static ulong NetworkToHostOrder(ulong a)
		{
			return (ushort)IPAddress.NetworkToHostOrder((long)a);
		}

		public static uint HostToNetworkOrder(uint a)
		{
			return (uint)IPAddress.HostToNetworkOrder((int)a);
		}

		public static ushort HostToNetworkOrder(ushort a)
		{
			return (ushort)IPAddress.HostToNetworkOrder((short)a);
		}

		public static ulong HostToNetworkOrder(ulong a)
		{
			return (ushort)IPAddress.HostToNetworkOrder((long)a);
		}
	}
}
