using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using BossCommand;
using BossBase;
using Helper;
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
		private string findType = "egametang@126.com";
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

		public async Task GetCharacterInfo()
		{
			ABossCommand bossCommand = new BCGetCharacterInfo(this.IMessageChannel)
			{
				FindTypeIndex = this.FindTypeIndex, 
				FindType = this.FindType
			};
			var result = await bossCommand.DoAsync();
			if (result == null)
			{
				this.ErrorInfo = string.Format("获取玩家信息失败");
				return;
			}
			var characterInfo = (CharacterInfo)result;

			this.ErrorInfo = "获取玩家信息成功";
			this.Account = characterInfo.Account;
			this.Name = characterInfo.Name;
			this.Guid = characterInfo.Guid.ToString();
		}

		public async Task ForbidCharacter(string forbiddenCommand, string forbiddenTime)
		{
			if (this.Guid == "")
			{
				this.ErrorInfo = "请先指定玩家";
				return;
			}

			int time = 0;
			if (!int.TryParse(forbiddenTime, out time))
			{
				this.ErrorInfo = "时间请输入数字";
				return;
			}

			ABossCommand bossCommand = new BCForbiddenCharacter(this.IMessageChannel)
			{
				Guid = this.Guid,
				Command = forbiddenCommand,
				ForbiddenTime = forbiddenTime
			};
			var result = await bossCommand.DoAsync();

			var errorCode = (uint)result;

			if (errorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				this.ErrorInfo = string.Format(
					"{0} {1} {2} Succeed!", forbiddenCommand, this.Guid, forbiddenTime);
				return;
			}
			this.ErrorInfo = string.Format("{0} Fail, error code: {1}", forbiddenCommand, errorCode);
		}

		public async Task ForbiddenLogin(
			string forbiddenCommand, string forbiddenContent, string forbiddenTime)
		{
			int time = 0;
			if (!int.TryParse(forbiddenTime, out time))
			{
				this.ErrorInfo = "时间请输入数字";
				return;
			}

			ABossCommand bossCommand = new BCForbidLogin(this.IMessageChannel)
			{
				Command = forbiddenCommand,
				Content = forbiddenContent,
				ForbiddenLoginTime = forbiddenTime
			};
			var result = await bossCommand.DoAsync();

			var errorCode = (uint)result;

			if (errorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				this.ErrorInfo = string.Format(
					"{0} {1} {2} Succeed!", forbiddenCommand, forbiddenContent, forbiddenTime);
				return;
			}
			this.ErrorInfo = string.Format("{0}, error code: {1}", forbiddenCommand, errorCode);
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
			var smsgBossCommandResponse = (SMSG_Boss_Command_Response)result;
			this.ErrorInfo = string.Format(" send command: {0}, error code: {1}", 
				commandString, JsonHelper.ToString(smsgBossCommandResponse));
		}
	}
}