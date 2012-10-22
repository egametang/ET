using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public class Event
	{
		private readonly ENetEvent ev;

		public Event(ENetEvent ev)
		{
			this.ev = ev;
		}

		public ENetEvent Ev
		{
			get
			{
				return this.ev;
			}
		}

		public Packet Packet
		{
			get
			{
				return new Packet(this.Ev.packet);
			}
		}

		public Peer Peer
		{
			get
			{
				return new Peer(this.Ev.peer);
			}
		}

		public EventType Type
		{
			get
			{
				return this.Ev.type;
			}
		}
	}
}