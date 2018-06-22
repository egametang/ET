using ProtoBuf;
using ETModel;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace ETHotfix
{
	[Message(HotfixOpcode.C2R_Login)]
	[ProtoContract]
	public partial class C2R_Login: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Account;

		[ProtoMember(2, IsRequired = true)]
		public string Password;

	}

	[Message(HotfixOpcode.R2C_Login)]
	[ProtoContract]
	public partial class R2C_Login: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Address;

		[ProtoMember(2, IsRequired = true)]
		public long Key;

	}

	[Message(HotfixOpcode.C2G_LoginGate)]
	[ProtoContract]
	public partial class C2G_LoginGate: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(HotfixOpcode.G2C_LoginGate)]
	[ProtoContract]
	public partial class G2C_LoginGate: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

	}

	[Message(HotfixOpcode.G2C_TestHotfixMessage)]
	[ProtoContract]
	public partial class G2C_TestHotfixMessage: IMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(HotfixOpcode.C2M_TestActorRequest)]
	[ProtoContract]
	public partial class C2M_TestActorRequest: IActorRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(93, IsRequired = true)]
		public long ActorId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(HotfixOpcode.M2C_TestActorResponse)]
	[ProtoContract]
	public partial class M2C_TestActorResponse: IActorResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(HotfixOpcode.PlayerInfo)]
	[ProtoContract]
	public partial class PlayerInfo: IMessage
	{
	}

	[Message(HotfixOpcode.C2G_PlayerInfo)]
	[ProtoContract]
	public partial class C2G_PlayerInfo: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

	}

	[Message(HotfixOpcode.G2C_PlayerInfo)]
	[ProtoContract]
	public partial class G2C_PlayerInfo: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();

	}

	[Message(HotfixOpcode.C2R_Register_Request)]
	[ProtoContract]
	public partial class C2R_Register_Request: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Account;

		[ProtoMember(2, IsRequired = true)]
		public string Password;

		[ProtoMember(3, IsRequired = true)]
		public string SafeQuestion;

		[ProtoMember(4, IsRequired = true)]
		public string SafeAnswer;

	}

	[Message(HotfixOpcode.R2C_Register_Response)]
	[ProtoContract]
	public partial class R2C_Register_Response: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(HotfixOpcode.C2R_Login_Request)]
	[ProtoContract]
	public partial class C2R_Login_Request: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Account;

		[ProtoMember(2, IsRequired = true)]
		public string Password;

	}

	[Message(HotfixOpcode.R2C_Login_Response)]
	[ProtoContract]
	public partial class R2C_Login_Response: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(HotfixOpcode.C2G_LoginGate_Request)]
	[ProtoContract]
	public partial class C2G_LoginGate_Request: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(HotfixOpcode.G2C_LoginGate_Response)]
	[ProtoContract]
	public partial class G2C_LoginGate_Response: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long PlayerID;

		[ProtoMember(2, IsRequired = true)]
		public long UserID;

		[ProtoMember(3, IsRequired = true)]
		public LoginBasicRoleInfo Info;

	}

	[Message(HotfixOpcode.C2G_CrateCharacter_Request)]
	[ProtoContract]
	public partial class C2G_CrateCharacter_Request: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string PlayerName;

		[ProtoMember(2, IsRequired = true)]
		public int CharacterID;

		[ProtoMember(3, IsRequired = true)]
		public int StrBp;

		[ProtoMember(4, IsRequired = true)]
		public int HealthBP;

		[ProtoMember(5, IsRequired = true)]
		public int DefBP;

		[ProtoMember(6, IsRequired = true)]
		public int SpeedBP;

		[ProtoMember(7, IsRequired = true)]
		public int MagicBP;

		[ProtoMember(8, IsRequired = true)]
		public int Di;

		[ProtoMember(9, IsRequired = true)]
		public int Shui;

		[ProtoMember(10, IsRequired = true)]
		public int Huo;

		[ProtoMember(11, IsRequired = true)]
		public int Feng;

		[ProtoMember(12, IsRequired = true)]
		public long UserID;

	}

	[Message(HotfixOpcode.G2C_CrateCharacter_Response)]
	[ProtoContract]
	public partial class G2C_CrateCharacter_Response: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public LoginBasicRoleInfo Info;

	}

	[Message(HotfixOpcode.LoginBasicRoleInfo)]
	[ProtoContract]
	public partial class LoginBasicRoleInfo: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public RoleInfo RoleInfo;

		[ProtoMember(2, IsRequired = true)]
		public RoleStateInfo StateInfo;

		[ProtoMember(3, IsRequired = true)]
		public List<TitleInfo> TitleInfo;

	}

	[Message(HotfixOpcode.RoleInfo)]
	[ProtoContract]
	public partial class RoleInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public string Name;

		[ProtoMember(2, IsRequired = true)]
		public int VipDays;

		[ProtoMember(3, IsRequired = true)]
		public int Level;

		[ProtoMember(4, IsRequired = true)]
		public int Job;

		[ProtoMember(5, IsRequired = true)]
		public int JobLevel;

		[ProtoMember(6, IsRequired = true)]
		public long Coin;

		[ProtoMember(7, IsRequired = true)]
		public long LockCoin;

		[ProtoMember(10, IsRequired = true)]
		public int CharacterID;

		[ProtoMember(11, IsRequired = true)]
		public string CustomTitle;

		[ProtoMember(12, IsRequired = true)]
		public int SecletTitleId;

		[ProtoMember(13, IsRequired = true)]
		public string MapID;

		[ProtoMember(14, IsRequired = true)]
		public int Dong;

		[ProtoMember(15, IsRequired = true)]
		public int Nan;

		[ProtoMember(16, IsRequired = true)]
		public int NowExp;

		[ProtoMember(17, IsRequired = true)]
		public int BPLeft;

		[ProtoMember(18, IsRequired = true)]
		public int HealthBP;

		[ProtoMember(19, IsRequired = true)]
		public int StrBP;

		[ProtoMember(20, IsRequired = true)]
		public int DefBP;

		[ProtoMember(21, IsRequired = true)]
		public int SpeedBP;

		[ProtoMember(22, IsRequired = true)]
		public int MagicBP;

		[ProtoMember(23, IsRequired = true)]
		public int HealthState;

		[ProtoMember(24, IsRequired = true)]
		public bool IsFront;

		[ProtoMember(25, IsRequired = true)]
		public int Meili;

		[ProtoMember(26, IsRequired = true)]
		public int Naili;

		[ProtoMember(27, IsRequired = true)]
		public int Zhili;

		[ProtoMember(28, IsRequired = true)]
		public int Lingqiao;

	}

	[Message(HotfixOpcode.RoleStateInfo)]
	[ProtoContract]
	public partial class RoleStateInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public float HPNow;

		[ProtoMember(2, IsRequired = true)]
		public float HPTotal;

		[ProtoMember(3, IsRequired = true)]
		public float MPNow;

		[ProtoMember(4, IsRequired = true)]
		public float MPTotal;

//
		[ProtoMember(5, IsRequired = true)]
		public int Zhanji;

		[ProtoMember(6, IsRequired = true)]
		public int Rongyao;

		[ProtoMember(7, IsRequired = true)]
		public int ZhongZhu;

//
		[ProtoMember(8, IsRequired = true)]
		public float Attack;

		[ProtoMember(9, IsRequired = true)]
		public float Defend;

		[ProtoMember(10, IsRequired = true)]
		public float Speed;

		[ProtoMember(11, IsRequired = true)]
		public float Jingshen;

		[ProtoMember(12, IsRequired = true)]
		public float Huifu;

//
		[ProtoMember(13, IsRequired = true)]
		public int Kangdu;

		[ProtoMember(14, IsRequired = true)]
		public int Kanghunshui;

		[ProtoMember(15, IsRequired = true)]
		public int Kangshihua;

		[ProtoMember(16, IsRequired = true)]
		public int Kangjiuzui;

		[ProtoMember(17, IsRequired = true)]
		public int Kanghunluan;

		[ProtoMember(18, IsRequired = true)]
		public int Kangyiwang;

//
		[ProtoMember(19, IsRequired = true)]
		public int Bisha;

		[ProtoMember(20, IsRequired = true)]
		public int Fanji;

		[ProtoMember(21, IsRequired = true)]
		public int Minzhong;

		[ProtoMember(22, IsRequired = true)]
		public int Mogong;

//
		[ProtoMember(23, IsRequired = true)]
		public int Di;

		[ProtoMember(24, IsRequired = true)]
		public int Shui;

		[ProtoMember(25, IsRequired = true)]
		public int Huo;

		[ProtoMember(26, IsRequired = true)]
		public int Feng;

	}

	[Message(HotfixOpcode.FriendInfo)]
	[ProtoContract]
	public partial class FriendInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public long UserID;

		[ProtoMember(2, IsRequired = true)]
		public string Name;

		[ProtoMember(3, IsRequired = true)]
		public int Level;

	}

	[Message(HotfixOpcode.ItemInfo)]
	[ProtoContract]
	public partial class ItemInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public List<int[]> Info;

	}

	[Message(HotfixOpcode.PetInfo)]
	[ProtoContract]
	public partial class PetInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public List<string> Name;

		[ProtoMember(2, IsRequired = true)]
		public List<int[]> Info;

	}

	[Message(HotfixOpcode.BankItemInfo)]
	[ProtoContract]
	public partial class BankItemInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public List<int[]> Info;

	}

	[Message(HotfixOpcode.BankPetInfo)]
	[ProtoContract]
	public partial class BankPetInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public List<string> Name;

		[ProtoMember(2, IsRequired = true)]
		public List<int[]> Info;

	}

	[Message(HotfixOpcode.SkillInfo)]
	[ProtoContract]
	public partial class SkillInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public List<int[]> Skill;

	}

	[Message(HotfixOpcode.TitleInfo)]
	[ProtoContract]
	public partial class TitleInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public int TitleID;

		[ProtoMember(2, IsRequired = true)]
		public int TitleType;

	}

}
