using System;

namespace ENet
{
	public enum EventState
	{
		CONNECTED = 0,
		DISCONNECTED = 1,
	}

	public class Event
	{
		private readonly ENetEvent ev;
		private EventState peerState = EventState.CONNECTED;

		public Event(ENetEvent ev)
		{
			this.ev = ev;
		}

		public EventState EventState
		{
			get
			{
				return peerState;
			}
			set
			{
				peerState = value;
			}
		}

		public ENetEvent Ev
		{
			get
			{
				return this.ev;
			}
		}

		public IntPtr PacketPtr
		{
			get
			{
				return Ev.packet;
			}
		}

		public IntPtr PeerPtr
		{
			get
			{
				return this.Ev.peer;
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