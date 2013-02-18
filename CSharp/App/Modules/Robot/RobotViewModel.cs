using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Threading;
using BossClient;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal sealed class RobotViewModel: NotificationObject, IDisposable
	{
		private string loginIP = "192.168.11.95";
		private ushort loginPort = 8888;
		private string account = "egametang@163.com";
		private string password = "163bio1";
		private string command = "";
		private bool isEnableSendCommandButton;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();

		private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal)
		{ Interval = new TimeSpan(0, 0, 0, 0, 50) };

		public string LoginIP
		{
			get
			{
				return this.loginIP;
			}
			set
			{
				if (this.loginIP == value)
				{
					return;
				}
				this.loginIP = value;
				this.RaisePropertyChanged("LoginIP");
			}
		}

		public ushort LoginPort
		{
			get
			{
				return this.loginPort;
			}
			set
			{
				if (this.loginPort == value)
				{
					return;
				}
				this.loginPort = value;
				this.RaisePropertyChanged("LoginPort");
			}
		}

		public string Account
		{
			get
			{
				return this.account;
			}
			set
			{
				if (this.account == value)
				{
					return;
				}
				this.account = value;
				this.RaisePropertyChanged("Account");
			}
		}

		public string Password
		{
			get
			{
				return this.password;
			}
			set
			{
				if (this.password == value)
				{
					return;
				}
				this.password = value;
				this.RaisePropertyChanged("Password");
			}
		}

		public string Command
		{
			get
			{
				return this.command;
			}
			set
			{
				if (this.command == value)
				{
					return;
				}
				this.command = value;
				this.RaisePropertyChanged("Command");
			}
		}

		public bool IsEnableSendCommandButton
		{
			get
			{
				return this.isEnableSendCommandButton;
			}
			set
			{
				if (this.isEnableSendCommandButton == value)
				{
					return;
				}
				this.isEnableSendCommandButton = value;
				this.RaisePropertyChanged("IsEnableSendCommandButton");
			}
		}

		public RobotViewModel()
		{
			this.timer.Tick += delegate { this.bossClient.RunOnce(); };
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

		private void Disposing(bool disposing)
		{
			this.bossClient.Dispose();
		}

		public async Task Login()
		{
			await this.bossClient.Login(
				this.LoginIP, this.LoginPort, this.Account, this.Password);
			this.IsEnableSendCommandButton = true;
			this.bossClient.HandleMessages();
		}

		public void SendCommand()
		{
			this.bossClient.SendCommand(this.Command);
		}
	}
}