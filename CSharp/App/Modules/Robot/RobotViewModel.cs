using System;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using ENet;
using Log;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class RobotViewModel: NotificationObject
	{
		private readonly Host host;
		private string logText = "";

		private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal)
		{ Interval = new TimeSpan(0, 0, 0, 0, 50) };

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
			this.host = new Host();

			this.timer.Tick += delegate { this.host.Run(); };
			this.timer.Start();
		}

		public async void StartClient()
		{
			try
			{
				var address = new Address { Host = "192.168.10.246", Port = 8901 };
				using (Peer peer = await this.host.ConnectAsync(address))
				{
					using (Packet packet = await peer.ReceiveAsync())
					{
						Logger.Debug(packet.Length + " " + packet.Data);

						await peer.DisconnectLaterAsync();
					}
				}
			}
			catch (Exception e)
			{
				Logger.Debug(e.Message);
			}
		}

		public void Start()
		{
			for (int i = 0; i < 4095; ++i)
			{
				this.StartClient();
			}
		}
	}
}