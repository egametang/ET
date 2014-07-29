using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BossBase;
using Logger;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Login
{
    [Export(contractType: typeof (LoginViewModel)),
     PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
    internal class LoginViewModel: BindableBase
    {
        private IEventAggregator EventAggregator { get; set; }
        private string account = "";
        private string password = "";
        private string errorInfo = "";
        private Visibility loginWindowVisiable = Visibility.Visible;
        private readonly BossClient.BossClient bossClient = new BossClient.BossClient();

        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = new TimeSpan(0, 0, 0, 0, 50)
        };

        public Visibility LoginWindowVisiable
        {
            get
            {
                return this.loginWindowVisiable;
            }
            set
            {
                this.SetProperty(ref this.loginWindowVisiable, value);
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
                this.SetProperty(ref this.account, value);
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
                this.SetProperty(ref this.password, value);
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
                this.SetProperty(ref this.errorInfo, value);
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
                Log.Trace(e.ToString());
                return;
            }

            this.LoginWindowVisiable = Visibility.Hidden;
            this.EventAggregator.GetEvent<LoginOKEvent>()
                    .Publish(this.bossClient.GateSession.IMessageChannel);
        }
    }
}