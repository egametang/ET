using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using ENet;
using Log;
using Microsoft.Practices.Prism.ViewModel;
using LoginClient;

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
		private readonly LoginClient.LoginClient realmClient = new LoginClient.LoginClient();

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

		public RobotViewModel()
		{
			//this.timer.Tick += delegate { this.clientHost.RunOnce(); };
			//this.timer.Start();
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
		}

		public async void Login()
		{
			try
			{
				// 登录realm
				List<Realm_List_Gate> gateList = await this.realmClient.LoginRealm(
					this.LoginIP, this.LoginPort, this.Account, this.Password);

				// 登录gate
			}
			catch (Exception e)
			{
				Logger.Trace("realm exception: {0}, {1}", e.Message, e.StackTrace);
			}
		}
	}
}