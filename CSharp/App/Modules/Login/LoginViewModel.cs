using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BossBase;
using Log;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;

namespace Login
{
	[Export(contractType: typeof (LoginViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class LoginViewModel : NotificationObject
	{
		private IEventAggregator EventAggregator { get; set; }
		private string account = "";
		private string password = "";
		private string errorInfo = "";
		private Visibility loginWindowVisiable = Visibility.Visible;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();
		private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal) 
		{ Interval = new TimeSpan(0, 0, 0, 0, 50) };

		public Visibility LoginWindowVisiable
		{
			get
			{
				return this.loginWindowVisiable;
			}
			set
			{
				if (this.loginWindowVisiable == value)
				{
					return;
				}
				this.loginWindowVisiable = value;
				this.RaisePropertyChanged("LoginWindowVisiable");
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

		public string ErrorInfo
		{
			get
			{
				return this.errorInfo;
			}
			set
			{
				if (this.errorInfo == value)
				{
					return;
				}
				this.errorInfo = value;
				this.RaisePropertyChanged("ErrorInfo");
			}
		}

		[ImportingConstructor]
		public LoginViewModel(IEventAggregator eventAggregator)
		{
			this.EventAggregator = eventAggregator;
			this.EventAggregator.GetEvent<ReLoginEvent>().Subscribe(this.OnReLoginEvent);
			this.timer.Tick += delegate { this.bossClient.RunOnce(); };
			this.timer.Start();
		}

		public void OnReLoginEvent(object obj)
		{
			this.LoginWindowVisiable = Visibility.Visible;
		}

		public async Task Login()
		{
			string ip = ConfigurationManager.AppSettings["IP"];
			ushort port = UInt16.Parse(ConfigurationManager.AppSettings["Port"]);

			if (this.Account == "")
			{
				this.Account = ConfigurationManager.AppSettings["Account"];
			}
			if (this.Password == "")
			{
				this.Password = ConfigurationManager.AppSettings["Password"];
			}

			try
			{
				await this.bossClient.Login(ip, port, this.Account, this.Password);
			}
			catch (Exception e)
			{
				this.ErrorInfo = "登录失败";
				Logger.Trace(e.ToString());
				return;
			}
			
			this.LoginWindowVisiable = Visibility.Hidden;
			this.EventAggregator.GetEvent<LoginOKEvent>().Publish(
				bossClient.GateSession.IMessageChannel);
		}
	}
}