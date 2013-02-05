using System;
using ENet;

namespace LoginClient
{
	public class GateSession: IDisposable
	{
		private readonly Peer peer;
		public int ID { get; set; }

		public GateSession(Peer peer)
		{
			this.peer = peer;
		}

		public void Dispose()
		{
			this.peer.Dispose();
		}

		public void Login()
		{
			
		}
	}
}
