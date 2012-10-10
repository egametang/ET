using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Threading;
using ELog;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;
using ENet;
using Infrastructure;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class RobotViewModel : NotificationObject
	{
		private ENetHost host;
		private string logText = "";
		private IEventAggregator eventAggregator = new EventAggregator();
		private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal)
		{
			Interval = new TimeSpan(0, 0, 0, 0, 50)
		};

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
			host = new ENetHost(8888, Native.ENET_PROTOCOL_MAXIMUM_PEER_ID);

			timer.Tick += delegate { this.host.Run(); };
			timer.Start();
		}

		public async void StartClient()
		{
			try
			{
				await host.ConnectAsync(new Address { Host = "192.168.10.246", Port = 8901 }, 2, 0);
			}
			catch (ENetException e)
			{
				Log.Debug(e.Message);
				return;
			}
		}

		public void Start()
		{
			for (int i = 0; i < 4095; ++i)
			{
				StartClient();
			}
		}
	}
}