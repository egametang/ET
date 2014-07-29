using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BossBase;
using Helper;
using Logger;

namespace BossCommand
{
    public class BCGetCharacterInfo: ABossCommand
    {
        public BCGetCharacterInfo(IMessageChannel iMessageChannel): base(iMessageChannel)
        {
        }

        public int FindTypeIndex { get; set; }

        public string FindType { get; set; }

        public override async Task<object> DoAsync()
        {
            this.CommandString = string.Format("get_character_info {0} {1} ", this.FindTypeIndex,
                    this.FindType);
            this.SendMessage(new CMSG_Boss_Gm { Message = this.CommandString });
            var smsgBossCommandResponse = await this.RecvMessage<SMSG_Boss_Command_Response>();
            if (smsgBossCommandResponse.ErrorCode != ErrorCode.RESPONSE_SUCCESS)
            {
                Log.Trace("get character info fail, error code: {0}",
                        smsgBossCommandResponse.ErrorCode);
                return null;
            }

            var characterInfo = MongoHelper.FromJson<CharacterInfo>(smsgBossCommandResponse.Content);
            return characterInfo;
        }
    }

    [DataContract]
    public class CharacterInfo
    {
        [DataMember(Order = 0, IsRequired = false)]
        public string Account { get; set; }

        [DataMember(Order = 1, IsRequired = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, IsRequired = false)]
        public UInt64 Guid { get; set; }
    }
}