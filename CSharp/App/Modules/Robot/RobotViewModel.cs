using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
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
		private int findTypeIndex;
		private string account = "";
		private string findType = "";
		private string name = "";
		private string guid = "";
		private Visibility dockPanelVisiable = Visibility.Hidden;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();
		private readonly ObservableCollection<ServerViewModel> serverInfos = 
			new ObservableCollection<ServerViewModel>();

		public readonly Dictionary<ushort, Action<byte[]>> messageHandlers =
			new Dictionary<ushort, Action<byte[]>>();

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

		[ImportingConstructor]
		public RobotViewModel(IEventAggregator eventAggregator)
		{
			this.messageHandlers.Add(
				MessageOpcode.SMSG_BOSS_SERVERSINFO, Handle_SMSG_Boss_ServersInfo);
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
			this.bossClient.Dispose();
		}

		public async void OnLoginOK(IMessageChannel messageChannel)
		{
			this.DockPanelVisiable = Visibility.Visible;
			this.IMessageChannel = messageChannel;
			try
			{
				while (true)
				{
					var result = await this.IMessageChannel.RecvMessage();
					ushort opcode = result.Item1;
					byte[] message = result.Item2;
					if (!messageHandlers.ContainsKey(opcode))
					{
						Logger.Debug("not found opcode: {0}", opcode);
						continue;
					}
					messageHandlers[opcode](message);
				}
			}
			catch (Exception e)
			{
				this.dockPanelVisiable = Visibility.Hidden;
				Logger.Trace(e.ToString());
			}
		}

		public void SendCommand(string command)
		{
			var cmsgBossGm = new CMSG_Boss_Gm
			{
				Message = command
			};

			this.IMessageChannel.SendMessage(MessageOpcode.CMSG_BOSS_GM, cmsgBossGm);
		}

		public void Servers()
		{
			this.SendCommand("servers");
		}

		public void Handle_SMSG_Boss_ServersInfo(byte[] message)
		{
			var smsgBossServersInfo = ProtobufHelper.FromBytes<SMSG_Boss_ServersInfo>(message);

			this.ServerInfos.Clear();
			foreach (var nm in smsgBossServersInfo.Name)
			{
				this.ServerInfos.Add(new ServerViewModel { Name = nm });
			}
		}

		public void Reload()
		{
			this.SendCommand("reload");
		}

		public void Find()
		{
			using (var entitys = new DataCenterEntities())
			{
				t_character result = null;
				switch (this.FindTypeIndex)
				{
					case 0:
					{
						result = entitys.t_character.FirstOrDefault(
							c => c.account == this.FindType);
						break;
					}
					case 1:
					{
						result = entitys.t_character.FirstOrDefault(
							c => c.character_name == this.FindType);
						break;
					}
					case 2:
					{
						var findGuid = Decimal.Parse(this.FindType);
						result = entitys.t_character.FirstOrDefault(
							c => c.character_guid == findGuid);
						break;
					}
				}
			
				if (result == null)
				{
					Logger.Debug("not find charactor info!");
					return;
				}

				this.Account = result.account;
				this.Name = result.character_name;
				this.Guid = result.character_guid.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}