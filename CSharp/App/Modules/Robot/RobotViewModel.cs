using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using BossCommand;
using BossBase;
using Log;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.Shared)]
	internal sealed class RobotViewModel: NotificationObject, IDisposable
	{
		private readonly IEventAggregator eventAggregator;

		private string errorInfo = "";
		private int findTypeIndex;
		private string account = "";
		private string findType = "";
		private string name = "";
		private string guid = "";
		private string command = "";
		private bool isGMEnable;
		private Visibility dockPanelVisiable = Visibility.Hidden;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();
		private readonly ObservableCollection<ServerViewModel> serverInfos = 
			new ObservableCollection<ServerViewModel>();

		public IMessageChannel IMessageChannel { get; set; }

		public int FindTypeIndex
		{
			get
			{
				return this.findTypeIndex;
			}
			set
			{
				if (this.findTypeIndex == value)
				{
					return;
				}
				this.findTypeIndex = value;
				this.RaisePropertyChanged("FindTypeIndex");
			}
		}

		public string FindType
		{
			get
			{
				return this.findType;
			}
			set
			{
				if (this.findType == value)
				{
					return;
				}
				this.findType = value;
				this.RaisePropertyChanged("FindType");
			}
		}

		public Visibility DockPanelVisiable
		{
			get
			{
				return this.dockPanelVisiable;
			}
			set
			{
				if (this.dockPanelVisiable == value)
				{
					return;
				}
				this.dockPanelVisiable = value;
				this.RaisePropertyChanged("DockPanelVisiable");
			}
		}

		public ObservableCollection<ServerViewModel> ServerInfos
		{
			get
			{
				return this.serverInfos;
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

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (this.name == value)
				{
					return;
				}
				this.name = value;
				this.RaisePropertyChanged("Name");
			}
		}

		public string Guid
		{
			get
			{
				return this.guid;
			}
			set
			{
				if (this.guid == value)
				{
					return;
				}
				this.guid = value;
				this.RaisePropertyChanged("Guid");
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

		public bool IsGMEnable
		{
			get
			{
				return this.isGMEnable;
			}
			set
			{
				if (this.isGMEnable == value)
				{
					return;
				}
				this.isGMEnable = value;
				this.RaisePropertyChanged("IsGMEnable");
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

		[ImportingConstructor]
		public RobotViewModel(IEventAggregator eventAggregator)
		{
			this.eventAggregator = eventAggregator;
			eventAggregator.GetEvent<LoginOKEvent>().Subscribe(this.OnLoginOKEvent);
		}

		~RobotViewModel()
		{
			this.Disposing();
		}

		public void Dispose()
		{
			this.Disposing();
			GC.SuppressFinalize(this);
		}

		private void Disposing()
		{
			this.bossClient.Dispose();
		}

		public void OnLoginOKEvent(IMessageChannel messageChannel)
		{
			this.DockPanelVisiable = Visibility.Visible;
			this.IMessageChannel = messageChannel;
		}

		public void ReLogin()
		{
			this.DockPanelVisiable = Visibility.Hidden;
			this.eventAggregator.GetEvent<ReLoginEvent>().Publish(null);
		}

		public async Task Servers()
		{
			ABossCommand bossCommand = new BCServerInfo(this.IMessageChannel);
			var result = await bossCommand.DoAsync();

			var smsgBossServersInfo = result as SMSG_Boss_ServersInfo;
			if (smsgBossServersInfo == null)
			{
				this.ErrorInfo = "查询服务器失败!";
				return;
			}

			this.ServerInfos.Clear();
			foreach (var nm in smsgBossServersInfo.Name)
			{
				this.ServerInfos.Add(new ServerViewModel { Name = nm });
			}
			this.ErrorInfo = "查询服务器成功!";
		}

		public void Reload()
		{
			ABossCommand bossCommand = new BCReloadWorld(this.IMessageChannel);
			bossCommand.DoAsync();
		}

		public void FindPlayer()
		{
			ABossCommand bossCommand = new BCFindPlayer(this.IMessageChannel)
			{
				FindTypeIndex = this.FindTypeIndex, 
				FindType = this.FindType
			};
			bossCommand.DoAsync();
		}

		public async Task ForbiddenBuy()
		{
			if (this.Guid == "")
			{
				this.ErrorInfo = "请先指定玩家";
				return;
			}

			ABossCommand bossCommand = new BCForbiddenBuy(this.IMessageChannel)
			{
				Guid = this.Guid
			};
			var result = await bossCommand.DoAsync();

			var errorCode = (int)result;

			if (errorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				this.ErrorInfo = "禁止交易成功";
				return;
			}
			this.ErrorInfo = string.Format("禁止交易失败, error code: {0}", errorCode);
		}

		public async Task AllowBuy()
		{
			if (this.Guid == "")
			{
				this.ErrorInfo = "请先指定玩家";
				return;
			}
			ABossCommand bossCommand = new BCAllowBuy(this.IMessageChannel)
			{
				Guid = this.Guid
			};
			var result = await bossCommand.DoAsync();
			var errorCode = (int) result;
			if (errorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				this.ErrorInfo = "允许交易成功";
				return;
			}

			this.ErrorInfo = errorCode.ToString();
		}

		public async Task SendCommand()
		{
			if (this.Command.StartsWith("gm ", true, CultureInfo.CurrentCulture))
			{
				this.Command = this.Command.Substring(3);
			}
			ABossCommand bossCommand = new BCCommand(this.IMessageChannel)
			{ Command = this.Command };
			string commandString = this.Command;
			object result = null;
			try
			{
				result = await bossCommand.DoAsync();
			}
			catch(Exception e)
			{
				Logger.Trace(e.ToString());
				return;
			}
			var errorCode = (uint)result;
			this.ErrorInfo = string.Format(" send command: {0}, error code: {1}", 
				commandString, errorCode);
		}
	}
}