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
		private Host host;
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

			Task.Factory.StartNew(() =>
				{
				
				});


		}

		public async Task<Peer> StartClient()
		{
			
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