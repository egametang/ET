using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Threading;
using ENet;
using Log;
using Microsoft.Practices.Prism.ViewModel;
using ProtoBuf;
using Robot.Protos;

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
						var bytes = Encoding.Default.GetBytes(packet.Data);
						var builder = new StringBuilder();
						foreach (var b in bytes)
						{
							builder.Append(b.ToString("X2"));
						}
						Logger.Debug(string.Format("HEX string: {0}", builder));
						var smsg = Serializer.Deserialize<SMSG_Auth_Challenge>(new MemoryStream(bytes));
						Logger.Debug(string.Format("{0}, {1}", smsg.Num, smsg.Seed));
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
			for (int i = 0; i < 1; ++i)
			{
				this.StartClient();
			}
		}
	}
}