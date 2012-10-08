using System;
using System.ComponentModel.Composition;
using ELog;
using Microsoft.Practices.Prism.ViewModel;
using ENet;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class RobotViewModel : NotificationObject
	{
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

		public void Start()
		{
			var address = new Address {HostName = "192.168.10.246", Port = 8888};
			var host = new Host(address, Native.ENET_PROTOCOL_MAXIMUM_PEER_ID);
			var e = new Event();
			var peer = host.Connect(address, 255, 0);
			while (host.CheckEvents(out e) > 0)
			{
				if (e.Type == EventType.Connect)
				{
					LogText += ("Connect OK\r\n");
				}
			}
		}
	}
}