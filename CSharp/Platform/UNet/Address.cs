using System;
using System.Net;

namespace UNet
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
				IPAddress[] addresslist = Dns.GetHostAddresses(value);
				foreach (IPAddress address in addresslist)
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
				var address = new ENetAddress { Host = this.ip, Port = this.port };
				return address;
			}
		}
	}
}