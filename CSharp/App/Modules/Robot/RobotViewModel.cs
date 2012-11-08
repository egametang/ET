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
			this.host.Dispose();
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
						var builder = new StringBuilder();
						var bytes = packet.Bytes;
						for (int i = 0; i < bytes.Length; ++i)
						{
							var b = bytes[i];
							builder.Append(b.ToString("X2"));
						}
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