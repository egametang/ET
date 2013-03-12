using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using BossClient;
using BossCommand;
using DataCenter;
using BossBase;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.Shared)]
	internal sealed class RobotViewModel: NotificationObject, IDisposable
	{
		private string errorInfo = "";
		private int findTypeIndex;
		private string account = "";
		private string findType = "egametang@163.com";
		private string name = "";
		private string guid = "";
		private bool isGMEnable;
		private Visibility dockPanelVisiable = Visibility.Hidden;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();
		private readonly ObservableCollection<ServerViewModel> serverInfos = 
			new ObservableCollection<ServerViewModel>();

		public DataCenterEntities Entities { get; set; }

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

		[ImportingConstructor]
		public RobotViewModel(IEventAggregator eventAggregator)
		{
			this.Entities = new DataCenterEntities();
			eventAggregator.GetEvent<LoginOKEvent>().Subscribe(this.OnLoginOK);
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
			this.Entities.Dispose();
			this.bossClient.Dispose();
		}

		public void OnLoginOK(IMessageChannel messageChannel)
		{
			this.DockPanelVisiable = Visibility.Visible;
			this.IMessageChannel = messageChannel;
		}

		public async Task Servers()
		{
			ABossCommand bossCommand = new BCServerInfo(this.IMessageChannel, this.Entities);
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
			ABossCommand bossCommand = new BCReloadWorld(this.IMessageChannel, this.Entities);
			bossCommand.Do();
		}

		public void FindPlayer()
		{
			ABossCommand bossCommand = new BCFindPlayer(this.IMessageChannel, this.Entities)
			{
				FindTypeIndex = this.FindTypeIndex, 
				FindType = this.FindType
			};
			var result = bossCommand.Do() as t_character;

			if (result == null)
			{
				this.ErrorInfo = "查询失败";
				return;
			}

			this.Account = result.account;
			this.Name = result.character_name;
			this.Guid = result.character_guid.ToString(CultureInfo.InvariantCulture);
			this.IsGMEnable = true;
			this.ErrorInfo = "查询成功";
		}

		public async Task ForbiddenBuy()
		{
			if (this.Guid == "")
			{
				this.ErrorInfo = "请先指定玩家";
				return;
			}

			ABossCommand bossCommand = new BCForbiddenBuy(this.IMessageChannel, this.Entities)
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
			ABossCommand bossCommand = new BCAllowBuy(this.IMessageChannel, this.Entities)
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
	}
}