using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using ELog;
using Microsoft.Practices.Prism.ViewModel;
using ENet;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class RobotViewModel : NotificationObject
	{
		private Host host = null;
		private string logText = "";

		public string LogText
		{
			get
			{
				return this.logText;
			}
			set
			{
				if (this.logText == value)
				{
					return;
				}
				this.logText = value;
				this.RaisePropertyChanged("LogText");
			}
		}

		public RobotViewModel()
		{
			Library.Initialize();

			host = new Host(null, Native.ENET_PROTOCOL_MAXIMUM_PEER_ID);

			Task.Factory.StartNew(() =>
				{
					while (host.Service(10) >= 0)
					{
						Event e;
						while (host.CheckEvents(out e) > 0)
						{
							switch (e.Type)
							{
								case EventType.Receive:
								{
									LogText += "Receive OK\r\n";
									Log.Debug("receive ok");
									break;
								}
								case EventType.Disconnect:
								{
									e.Peer.Dispose();
									break;
								}
							}
						}
					}
				});


		}

		public async Task<Peer> StartClient()
		{
			return await Task.Factory.StartNew<Peer>(() =>
				{
					var address = new Address {Host = "192.168.10.246", Port = 8901};
					var peer = this.host.Connect(address, 2, 1);
					return peer;
				});
		}

		public void Start()
		{
			var peer = StartClient().Result;
			if (peer.State == PeerState.Connected)
			{
				Log.Debug("11111111111");
			}
		}
	}
}