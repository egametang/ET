using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Events;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.Login
{
	[Export(contractType: typeof (LoginViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class LoginViewModel : NotificationObject
	{
		private IEventAggregator EventAggregator { get; set; }
		private string account = "egametang@126.com";
		private string password = "163bio1";
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

		[ImportingConstructor]
		public LoginViewModel(IEventAggregator eventAggregator)
		{
			this.EventAggregator = eventAggregator;
			this.timer.Tick += delegate { this.bossClient.RunOnce(); };
			this.timer.Start();
		}

		public async Task Login()
		{
			string ip = ConfigurationManager.AppSettings["IP"];
			ushort port = UInt16.Parse(ConfigurationManager.AppSettings["Port"]);
			await this.bossClient.Login(ip, port, this.Account, this.Password);
			this.LoginWindowVisiable = Visibility.Hidden;
			this.EventAggregator.GetEvent<LoginOKEvent>().Publish(
				bossClient.GateSession.IMessageChannel);
		}
	}
}