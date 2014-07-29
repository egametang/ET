using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using BossBase;
using BossCommand;
using Helper;
using Logger;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Robot
{
    [Export(contractType: typeof (RobotViewModel)),
     PartCreationPolicy(creationPolicy: CreationPolicy.Shared)]
    internal sealed class RobotViewModel: BindableBase, IDisposable
    {
        private readonly IEventAggregator eventAggregator;

        private string errorInfo = "";
        private int findTypeIndex;
        private string account = "";
        private string findType = "egametang@126.com";
        private string name = "";
        private string guid = "";
        private string command = "";

        private string subject = "";
        private string content = "";
        private string freeGold = "";
        private string silver = "";
        private string itemID = "";
        private string itemCount = "";

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
                this.SetProperty(ref this.findTypeIndex, value);
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
                this.SetProperty(ref this.findType, value);
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
                this.SetProperty(ref this.dockPanelVisiable, value);
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
                this.SetProperty(ref this.account, value);
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
                this.SetProperty(ref this.name, value);
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
                this.SetProperty(ref this.guid, value);
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

        public string Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.SetProperty(ref this.command, value);
            }
        }

        public string Subject
        {
            get
            {
                return this.subject;
            }
            set
            {
                this.SetProperty(ref this.subject, value);
            }
        }

        public string Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.SetProperty(ref this.content, value);
            }
        }

        public string FreeGold
        {
            get
            {
                return this.freeGold;
            }
            set
            {
                this.SetProperty(ref this.freeGold, value);
            }
        }

        public string Silver
        {
            get
            {
                return this.silver;
            }
            set
            {
                this.SetProperty(ref this.silver, value);
            }
        }

        public string ItemID
        {
            get
            {
                return this.itemID;
            }
            set
            {
                this.SetProperty(ref this.itemID, value);
            }
        }

        public string ItemCount
        {
            get
            {
                return this.itemCount;
            }
            set
            {
                this.SetProperty(ref this.itemCount, value);
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

        public void ShowErrorInfo(uint errorCode, string commandString)
        {
            if (errorCode == ErrorCode.RESPONSE_SUCCESS)
            {
                this.ErrorInfo = string.Format("{0} Succeed!", commandString);
                return;
            }
            this.ErrorInfo = string.Format("{0} Fail, error code: {1}", commandString, errorCode);
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
                this.ErrorInfo = string.Format("{0} Fail!", bossCommand.CommandString);
                return;
            }
            var characterInfo = (CharacterInfo) result;

            this.Account = characterInfo.Account;
            this.Name = characterInfo.Name;
            this.Guid = characterInfo.Guid.ToString();
            this.ErrorInfo = string.Format("{0} Succeed!", bossCommand.CommandString);
        }

        public async Task ForbidCharacter(string forbiddenCommand, string forbiddenTime)
        {
            if (this.Guid == "")
            {
                this.ErrorInfo = "请先指定玩家";
                return;
            }
            ulong ulongGuid = 0;
            if (!ulong.TryParse(this.Guid, out ulongGuid))
            {
                this.ErrorInfo = "Guid必须是数字";
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
                Command = forbiddenCommand,
                Guid = this.Guid,
                ForbiddenTime = forbiddenTime
            };
            var result = await bossCommand.DoAsync();

            var errorCode = (uint) result;

            this.ShowErrorInfo(errorCode, bossCommand.CommandString);
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

            var errorCode = (uint) result;

            this.ShowErrorInfo(errorCode, bossCommand.CommandString);
        }

        public async Task SendMail()
        {
            BossMail bossMail = null;
            try
            {
                bossMail = new BossMail
                {
                    sender_name = "系统",
                    receiver_guid = ulong.Parse(this.Guid),
                    subject = this.Subject,
                    content = this.Content,
                    free_gold = uint.Parse(this.FreeGold),
                    silver = uint.Parse(this.Silver),
                    item_dict = new Dictionary<int, int>()
                };

                if (this.ItemID != "")
                {
                    bossMail.item_dict.Add(int.Parse(this.ItemID), int.Parse(this.ItemCount));
                }
            }
            catch (Exception e)
            {
                Log.Trace(e.ToString());
                this.ErrorInfo = "输入错误!";
                return;
            }

            ABossCommand bossCommand = new BCSendMail(this.IMessageChannel) { BossMail = bossMail };

            var result = await bossCommand.DoAsync();

            var errorCode = (uint) result;

            this.ShowErrorInfo(errorCode, bossCommand.CommandString);
        }

        public async Task SendCommand()
        {
            if (this.Command.StartsWith("gm ", true, CultureInfo.CurrentCulture))
            {
                this.Command = this.Command.Substring(3);
            }
            ABossCommand bossCommand = new BCCommand(this.IMessageChannel)
            {
                Command = this.Command
            };
            string commandString = this.Command;
            object result = null;
            try
            {
                result = await bossCommand.DoAsync();
            }
            catch (Exception e)
            {
                Log.Trace(e.ToString());
                return;
            }
            var smsgBossCommandResponse = (SMSG_Boss_Command_Response) result;
            this.ErrorInfo = string.Format(" send command: {0}, error code: {1}", commandString,
                    MongoHelper.ToJson(smsgBossCommandResponse));
        }
    }
}