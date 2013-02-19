using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Threading;
using BossClient;
using Helper;
using Log;
using Microsoft.Practices.Prism.ViewModel;

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
		private string command = "";
		private bool isButtonEnable;
		private readonly BossClient.BossClient bossClient = new BossClient.BossClient();
		private readonly ObservableCollection<ServerViewModel> serverInfos = 
			new ObservableCollection<ServerViewModel>();

		public readonly Dictionary<ushort, Action<byte[]>> messageHandlers =
			new Dictionary<ushort, Action<byte[]>>();

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

		public async Task Login()
		{
			await this.bossClient.Login(
				this.LoginIP, this.LoginPort, this.Account, this.Password);

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

		public void SendCommand()
		{
			this.bossClient.SendCommand(this.Command);
		}

		public void Servers()
		{
			this.bossClient.SendCommand("servers");
		}

		public void Reload()
		{
			this.bossClient.SendCommand("reload");
		}

		public void Handle_SMSG_Boss_ServersInfo(byte[] message)
		{
			var smsgBossServersInfo = ProtobufHelper.FromBytes<SMSG_Boss_ServersInfo>(message);

			this.ServerInfos.Clear();
			foreach (var name in smsgBossServersInfo.Name)
			{
				this.ServerInfos.Add(new ServerViewModel {Name = name});
			}
		}
	}
}