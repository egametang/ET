#region License

/*
ENet for C#
Copyright (c) 2011 James F. Bellinger <jfb@zer7.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

#endregion

using System;
using System.Net;
using System.Text;

namespace ENet
{
	public unsafe struct Address : IEquatable<Address>
	{
		public const uint IPv4HostAny = Native.ENET_HOST_ANY;
		public const uint IPv4HostBroadcast = Native.ENET_HOST_BROADCAST;

		private Native.ENetAddress address;

		public override bool Equals(object obj)
		{
			return obj is Address && this.Equals((Address) obj);
		}

		public bool Equals(Address addr)
		{
			return this.Port == addr.Port && this.IP == addr.IP;
		}

		public override int GetHashCode()
		{
			return this.Port.GetHashCode() ^ this.IP.GetHashCode();
		}

		public string IP
		{
			get
			{
				var ip = new byte[256];
				fixed (byte* hostIP = ip)
				{
					if (Native.enet_address_get_host_ip(ref this.address, hostIP, (uint)ip.Length) < 0)
					{
						return null;
					}
				}
				return Encoding.ASCII.GetString(ip, 0, Native.strlen(ip));
			}
		}

		public string Host
		{
			get
			{
				var name = new byte[256];
				fixed (byte* hostName = name)
				{
					if (Native.enet_address_get_host(ref this.address, hostName, (uint)name.Length) < 0)
					{
						return null;
					}
				}
				return Encoding.ASCII.GetString(name, 0, Native.strlen(name));
			}
			set
			{
				Native.enet_address_set_host(ref this.address, Encoding.ASCII.GetBytes(value));
			}
		}

		public Native.ENetAddress NativeData
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