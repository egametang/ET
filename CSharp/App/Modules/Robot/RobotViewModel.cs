using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BossClient;
using DataCenter;
using Events;
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

		private readonly DataCenterEntities entities = new DataCenterEntities();

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
			this.entities.Dispose();
			this.bossClient.Dispose();
		}

		public void OnLoginOK(IMessageChannel messageChannel)
		{
			this.DockPanelVisiable = Visibility.Visible;
			this.IMessageChannel = messageChannel;
		}

		public void SendCommand(string command)
		{
			var cmsgBossGm = new CMSG_Boss_Gm
			{
				Message = command
			};

			this.IMessageChannel.SendMessage(MessageOpcode.CMSG_BOSS_GM, cmsgBossGm);
		}

		public async Task<T> RecvMessage<T>()
		{
			var result = await this.IMessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] content = result.Item2;

			try
			{
				var message = ProtobufHelper.FromBytes<T>(content);
				return message;
			}
			catch (Exception)
			{
				Logger.Trace("parse message fail, opcode: {0}", opcode);
				throw;
			}
		}

		public async void Servers()
		{
			this.SendCommand("servers");
			var smsgBossServersInfo = await this.RecvMessage<SMSG_Boss_ServersInfo>();

			this.ServerInfos.Clear();
			foreach (var nm in smsgBossServersInfo.Name)
			{
				this.ServerInfos.Add(new ServerViewModel { Name = nm });
			}
			this.ErrorInfo = "查询服务器成功!";
		}

		public void Reload()
		{
			this.SendCommand("reload");
		}

		public void Find()
		{
			t_character result = null;
			switch (this.FindTypeIndex)
			{
				case 0:
				{
					result = entities.t_character.FirstOrDefault(
						c => c.account == this.FindType);
					break;
				}
				case 1:
				{
					result = entities.t_character.FirstOrDefault(
						c => c.character_name == this.FindType);
					break;
				}
				case 2:
				{
					var findGuid = Decimal.Parse(this.FindType);
					result = entities.t_character.FirstOrDefault(
						c => c.character_guid == findGuid);
					break;
				}
			}
			
			if (result == null)
			{
				this.ErrorInfo = "没有找到该玩家!";
				return;
			}
			
			this.Account = result.account;
			this.Name = result.character_name;
			this.Guid = result.character_guid.ToString(CultureInfo.InvariantCulture);
			this.IsGMEnable = true;
			this.ErrorInfo = "查询成功";
		}

		public async void ForbiddenBuy()
		{
			if (this.Guid == "")
			{
				this.ErrorInfo = "请先指定玩家";
				return;
			}
			this.SendCommand(string.Format("forbidden_buy_item {0} {1}", guid, int.MaxValue));
			var smsgBossCommandResponse = await RecvMessage<SMSG_Boss_Command_Response>();
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				this.ErrorInfo = "禁止交易成功";
				return;
			}
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.BOSS_PLAYER_NOT_FOUND)
			{
				decimal character_guid = decimal.Parse(this.Guid);
				var removeBuffs = entities.t_city_buff.Where(
					c => c.buff_id == BuffId.BUFF_FORBIDDEN_PLAYER_BUY_ITEM && 
						c.character_guid == character_guid);
				foreach (var removeBuff in removeBuffs)
				{
					entities.t_city_buff.Remove(removeBuff);
				}
				var newBuff = new t_city_buff
				{
					buff_guid = RandomHelper.RandUInt64(),
					buff_id = BuffId.BUFF_FORBIDDEN_PLAYER_BUY_ITEM,
					buff_time = 0,
					buff_values = "{}".ToByteArray(),
					character_guid = decimal.Parse(this.Guid),
					create_time = DateTime.Now,
					modify_time = DateTime.Now,
					stack = 1
				};
				entities.t_city_buff.Add(newBuff);
				entities.SaveChanges();

				this.ErrorInfo = "禁止交易成功";
				return;
			}
			this.ErrorInfo = smsgBossCommandResponse.ErrorCode.ToString();
		}

		public async void AllowBuy()
		{
			if (this.Guid == "")
			{
				this.ErrorInfo = "请先指定玩家";
				return;
			}
			this.SendCommand(string.Format("forbidden_buy_item {0} 0", guid));
			var smsgBossCommandResponse = await RecvMessage<SMSG_Boss_Command_Response>();
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				this.ErrorInfo = "允许交易成功";
				return;
			}
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.BOSS_PLAYER_NOT_FOUND)
			{
				decimal character_guid = decimal.Parse(this.Guid);
				var removeBuffs = entities.t_city_buff.Where(
					c => c.buff_id == BuffId.BUFF_FORBIDDEN_PLAYER_BUY_ITEM &&
						c.character_guid == character_guid);
				foreach (var removeBuff in removeBuffs)
				{
					entities.t_city_buff.Remove(removeBuff);
				}
				entities.SaveChanges();

				this.ErrorInfo = "允许交易成功";
				return;
			}
			this.ErrorInfo = smsgBossCommandResponse.ErrorCode.ToString();
		}
	}
}