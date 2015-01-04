using System;
using System.Net;

namespace UNet
{
	public struct UAddress
	{
		private uint ip;
		private ushort port;

		public uint IP
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

		public string Host
		{
			get
			{
				IPHostEntry hostInfo = Dns.GetHostEntry(new IPAddress(this.ip));
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
				ENetAddress address = new ENetAddress { Host = this.ip, Port = this.port };
				return address;
			}
		}
	}
}