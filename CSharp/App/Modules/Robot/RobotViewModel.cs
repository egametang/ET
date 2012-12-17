using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Threading;
using ENet;
using Helper;
using Log;
using Microsoft.Practices.Prism.ViewModel;
using ProtoBuf;
using Robot.Protos;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class RobotViewModel: NotificationObject, IDisposable
	{
		private readonly ClientHost clientHost;
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
			this.clientHost = new ClientHost();

			this.timer.Tick += delegate { this.clientHost.RunOnce(); };
			this.timer.Start();
		}

		~RobotViewModel()
		{
			this.Disposing(false);
		}

		public void Dispose()
		{
			this.Disposing(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Disposing(bool disposing)
		{	
			this.clientHost.Dispose();
		}

		public async void StartClient()
		{
			try
			{
				var address = new Address { HostName = "192.168.10.246", Port = 8900 };
				using (Peer peer = await this.clientHost.ConnectAsync(address))
				{
					using (Packet packet = await peer.ReceiveAsync())
					{
						var bytes = packet.Bytes;
						var packetStream = new MemoryStream(bytes, 4, bytes.Length - 4);
						var smsg = Serializer.Deserialize<SMSG_Auth_Challenge>(packetStream);
						Logger.Debug(string.Format(
							"opcode: {0}\n{1}", BitConverter.ToUInt16(bytes, 0), XmlHelper.XmlSerialize(smsg)));
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