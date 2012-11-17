using System;
using System.Net;

namespace ENet
{
	public struct Address
	{
		private uint ip;
		private ushort port;

		public uint Ip
		{
			get
			{
				return this.ip;
			}
			set
			{
				this.ip = value;
			}
		}

		public ushort Port
		{
			get
			{
				return this.port;
			}
			set
			{
				this.port = value;
			}
		}

		public string HostName
		{
			get
			{
				var hostInfo = Dns.GetHostEntry(new IPAddress(this.ip));
				return hostInfo.HostName;
			}
			set
			{
				var nameToIpAddress = Dns.GetHostEntry(value);
				foreach (IPAddress address in nameToIpAddress.AddressList)
				{
					this.ip = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
					return;
				}
			}
		}

		public ENetAddress Struct
		{
			get
			{
				var address = new ENetAddress { host = this.ip, port = this.port };
				return address;
			}
		}
	}
}