using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BossBase
{
	public static class MessageOpcode
	{
		// realm message opcode
		public const ushort CMSG_REALM_LIST = 16;
		public const ushort CMSG_BOSS_GM = 17;
		public const ushort CMSG_AUTH_LOGON_PERMIT = 800;
		public const ushort CMSG_OTP_PASSWORD = 801;
		public const ushort CMSG_PPC_PASSWORD = 802;
		public const ushort CMSG_AUTH_LOGON_CHALLENGE = 803;
		public const ushort CMSG_AUTH_LOGON_PROOF = 805;
		public const ushort SMSG_AUTH_LOGON_CHALLENGE_RESPONSE = 900;
		public const ushort SMSG_REALM_LOGON_RESPONSE = 901;
		public const ushort SMSG_REALM_LIST = 902;
		public const ushort SMSG_LOCK_FOR_SAFE_TIME = 903;
		public const ushort SMSG_PASSWORD_PROTECT_TYPE = 904;
		public const ushort SMSG_AUTH_LOGON_PROOF_M2 = 905;

		// gate message opcode
		public const ushort CMSG_AUTH_SESSION = 2;
		public const ushort SMSG_AUTH_CHALLENGE = 502;
		public const ushort SMSG_AUTH_RESPONSE = 503;

		public const ushort SMSG_SERVERTIME = 510;

		public const ushort SMSG_BOSS_SERVERSINFO = 22000;
		public const ushort SMSG_BOSS_COMMAND_RESPONSE = 22001;
	}

	public static class ErrorCode
	{
		// realm error code
		public const int REALM_AUTH_SUCCESS = 0;
		// Unable to connect
		public const int REALM_AUTH_FAILURE = 1;
		// Unable to connect
		public const int REALM_AUTH_UNKNOWN1 = 2;
		// This game> account has been closed and is no longer available for use.
		// Please go to site>/banned.html for further information.
		public const int REALM_AUTH_ACCOUNT_BANNED = 3;
		// The information you have entered is not valid.
		// Please check the spelling of the account name and password.
		// If you need help in retrieving a lost or stolen password,
		// see site> for more information
		public const int REALM_AUTH_NO_MATCH = 4;
		// The information you have entered is not valid.
		// Please check the spelling of the account name and password.
		// If you need help in retrieving a lost or stolen password,
		// see site> for more information
		public const int REALM_AUTH_UNKNOWN2 = 5;
		// This account is already logged into game.
		// Please check the spelling and try again.
		public const int REALM_AUTH_ACCOUNT_IN_USE = 6;
		// You have used up your prepaid time for this account.
		// Please purchase more to continue playing
		public const int REALM_AUTH_PREPAID_TIME_LIMIT = 7;
		// Could not log in to game> at this time. Please try again later.
		public const int REALM_AUTH_SERVER_FULL = 8;
		// Unable to validate game version.
		// This may be caused by file corruption or interference of another program.
		// Please visit site for more information and possible solutions to this
		// issue.
		public const int REALM_AUTH_WRONG_BUILD_NUMBER = 9;
		// Downloading
		public const int REALM_AUTH_UPDATE_CLIENT = 10;
		// Unable to connect
		public const int REALM_AUTH_UNKNOWN3 = 11;
		// This game account has been temporarily suspended.
		// Please go to site further information.
		public const int REALM_AUTH_ACCOUNT_FREEZED = 12;
		// Unable to connect
		public const int REALM_AUTH_UNKNOWN4 = 13;
		// Connected.
		public const int REALM_AUTH_UNKNOWN5 = 14;
		// Access to this account has been blocked by parental controls.
		// Your settings may be changed in your account preferences at site.
		public const int REALM_AUTH_PARENTAL_CONTROL = 15;


		// 其它error code
		public const int RESPONSE_SUCCESS = 0;
		public const int AUTH_OK = 12;
		public const int BOSS_PLAYER_NOT_FOUND = 164000;
	}

	[DataContract]
	public class CMSG_Auth_Logon_Permit
	{
		[DataMember(Order = 1, IsRequired = true)]
		public byte[] Account { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public byte[] PasswordMd5 { get; set; }
	}

	[DataContract]
	public class SMSG_Password_Protect_Type
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint Code { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public uint SubCode { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public uint PasswordProtectType { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public byte[] PpcCoordinate { get; set; }
	}

	[DataContract]
	public class CMSG_Auth_Logon_Challenge
	{
	}

	[DataContract]
	public class SMSG_Auth_Logon_Challenge_Response
	{
		[DataMember(Order = 1, IsRequired = true)]
		public int ErrorCode { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public byte[] B { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public byte[] G { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public byte[] N { get; set; }

		[DataMember(Order = 5, IsRequired = true)]
		public byte[] S { get; set; }
	}

	[DataContract]
	public class CMSG_Auth_Logon_Proof
	{
		[DataMember(Order = 1, IsRequired = true)]
		public byte[] A { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public byte[] M { get; set; }
	}

	[DataContract]
	public class CMSG_Realm_List
	{
	}

	[DataContract]
	public class SMSG_Auth_Logon_Proof_M2
	{
		[DataMember(Order = 1, IsRequired = true)]
		public int ErrorCode { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public byte[] M { get; set; }
	}

	[DataContract]
	public class Realm_List_Gate
	{
		[DataMember(Order = 1, IsRequired = true)]
		public byte[] Name { get; set; }
		[DataMember(Order = 2, IsRequired = true)]
		public byte[] Address { get; set; }
		[DataMember(Order = 3, IsRequired = true)]
		public float CityLoad { get; set; }
	}

	[DataContract]
	public class Realm_List_City
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string Name { get; set; } 

		[DataMember(Order = 2, IsRequired = true)]
		public float CityLoad { get; set; } 

		[DataMember(Order = 3, IsRequired = true)]
		public bool IsEnable { get; set; } 
	}

	[DataContract]
	public class SMSG_Realm_List
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string GateIP { get; set; } 

		[DataMember(Order = 2, IsRequired = true)]
		public int GatePort { get; set; } 

		[DataMember(Order = 3, IsRequired = true)]
		public List<Realm_List_City> GateList { get; set; } 
	}

	[DataContract]
	public class CMSG_Auth_Session 
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint ClientBuild { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public uint Unk2 { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public byte[] Username { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public uint Unk3 { get; set; }

		[DataMember(Order = 5, IsRequired = true)]
		public uint ClientSeed { get; set; }

		[DataMember(Order = 6, IsRequired = true)]
		public uint Unk4 { get; set; }

		[DataMember(Order = 7, IsRequired = true)]
		public byte[] Digest { get; set; }

		[DataMember(Order = 8, IsRequired = false)]
		public byte[] Mac { get; set; }

		[DataMember(Order = 9, IsRequired = false)]
		public byte[] Hd { get; set; }
	}

	// gate message
	[DataContract]
	public class SMSG_Auth_Challenge
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint Num { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public uint Seed { get; set; }

		[DataMember(Order = 3)]
		public List<uint> Random { get; set; }
	}

	[DataContract]
	public class SMSG_Auth_Response
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint ErrorCode { get; set; }
	}

	[DataContract]
	public class CMSG_Boss_Gm
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string Message { get; set; }
	}
	
	[DataContract]
	public class SMSG_Boss_ServersInfo
	{
		[DataMember(Order = 1, IsRequired = true)]
		public List<string> Name { get; set; }
	}

	[DataContract]
	public class SMSG_Boss_Command_Response
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint ErrorCode { get; set; }

		[DataMember(Order = 2, IsRequired = false)]
		public string Content { get; set; }
	}

	[DataContract]
	public class BossMailItem
	{
		[DataMember(Order = 1, IsRequired = false)]
		public uint item_id { get; set; }

		[DataMember(Order = 2, IsRequired = false)]
		public uint item_count { get; set; }
	}

	[DataContract]
	public class BossMail
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string sender_name { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public ulong receiver_guid { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public string subject { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public string content { get; set; }

		[DataMember(Order = 5, IsRequired = false)]
		public uint free_gold { get; set; }

		[DataMember(Order = 6, IsRequired = false)]
		public uint silver { get; set; }

		[DataMember(Order = 7, IsRequired = false)]
		public Dictionary<int, int> item_dict { get; set; }
	}
}