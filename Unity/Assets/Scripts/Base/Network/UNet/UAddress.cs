using System;
using System.Net;

namespace Model
{
	internal struct UAddress
	{
		private readonly uint ip;
		private readonly ushort port;

		public UAddress(string host, int port)
		{
			IPAddress address = IPAddress.Parse(host);
			this.ip = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
			this.port = (ushort) port;
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