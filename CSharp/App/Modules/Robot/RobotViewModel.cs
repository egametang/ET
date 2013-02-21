using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;
using BossClient;
using DataCenter;
using Helper;
using Log;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal sealed class RobotViewModel: NotificationObject, IDisposable
	{
		private string account = "egametang@163.com";
		private string password = "163bio1";
		private int findTypeIndex;
		private string findType = "";
		private bool isButtonEnable;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();
		private readonly ObservableCollection<ServerViewModel> serverInfos = 
			new ObservableCollection<ServerViewModel>();

		public readonly Dictionary<ushort, Action<byte[]>> messageHandlers =
			new Dictionary<ushort, Action<byte[]>>();

		private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal)
		{ Interval = new TimeSpan(0, 0, 0, 0, 50) };

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

		public bool IsButtonEnable
		{
			get
			{
				return this.isButtonEnable;
			}
			set
			{
				if (this.isButtonEnable == value)
				{
					return;
				}
				this.isButtonEnable = value;
				this.RaisePropertyChanged("IsButtonEnable");
			}
		}

		public ObservableCollection<ServerViewModel> ServerInfos
		{
			get
			{
				return this.serverInfos;
			}
		}

		public RobotViewModel()
		{
			this.messageHandlers.Add(
				MessageOpcode.SMSG_BOSS_SERVERSINFO, Handle_SMSG_Boss_ServersInfo);

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

		public async void Login()
		{
			var config = Config.Instance;
			await this.bossClient.Login(config.IP, config.Port, 
				this.Account, this.Password);

			this.IsButtonEnable = true;

			this.HandleMessages();
		}

		public async void HandleMessages()
		{
			try
			{
				while (true)
				{
					var result = await this.bossClient.GateSession.IMessageChannel.RecvMessage();
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
				this.IsButtonEnable = false;
				Logger.Trace(e.ToString());
			}
		}

		public void Servers()
		{
			this.bossClient.SendCommand("servers");
		}

		public void Handle_SMSG_Boss_ServersInfo(byte[] message)
		{
			var smsgBossServersInfo = ProtobufHelper.FromBytes<SMSG_Boss_ServersInfo>(message);

			this.ServerInfos.Clear();
			foreach (var name in smsgBossServersInfo.Name)
			{
				this.ServerInfos.Add(new ServerViewModel { Name = name });
			}
		}

		public void Reload()
		{
			this.bossClient.SendCommand("reload");
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
						var guid = Decimal.Parse(this.FindType);
						result = entitys.t_character.FirstOrDefault(
							c => c.character_guid == guid);
						break;
					}
				}
			
				if (result == null)
				{
					Logger.Debug("not find charactor info!");
					return;
				}
				
				Logger.Debug("Account: {0}, Name: {1}, GUID: {2}", 
					result.account, result.character_name, result.character_guid);
			}
		}
	}
}