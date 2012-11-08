using System;
using System.Text;

namespace ENet
{
	public struct Address: IEquatable<Address>
	{
		private ENetAddress address;

		public bool Equals(Address addr)
		{
			ENetAddress enetAddr = addr.Struct;
			return this.address.host == enetAddr.host && this.address.port == enetAddr.port;
		}

		public string IP
		{
			get
			{
				var hostIP = new StringBuilder(256);
				NativeMethods.enet_address_get_host_ip(ref this.address, hostIP, (uint) hostIP.Length);
				return hostIP.ToString();
			}
		}

		public string Host
		{
			get
			{
				var hostName = new StringBuilder(256);
				NativeMethods.enet_address_get_host(ref this.address, hostName, (uint) hostName.Length);
				return hostName.ToString();
			}
			set
			{
				NativeMethods.enet_address_set_host(ref this.address, value);
			}
		}

		public ENetAddress Struct
		{
			get
			{
				return this.address;
			}
		}

		public ushort Port
		{
			get
			{
				return this.address.port;
			}
			set
			{
				this.address.port = value;
			}
		}
	}
}