using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Robot.Protos
{
	public static class MessageOpcode
	{
		public const ushort CMSG_AUTHLOGONPERMIT = 800;
		public const ushort CMSG_OTP_PASSWORD = 801;
		public const ushort CMSG_PPC_PASSWORD = 802;
		public const ushort CMSG_AUTHLOGONCHALLENGE = 803;
		public const ushort CMSG_AUTHLOGONCHALLENGE_2 = 804;
		public const ushort CMSG_AUTHLOGONPROOF = 805;
		public const ushort SMSG_AUTH_LOGON_CHALLENGE_RESPONSE = 900;
		public const ushort SMSG_REALM_LOGON_RESPONSE = 901;
		public const ushort SMSG_REALM_LIST = 902;
		public const ushort SMSG_LOCK_FOR_SAFE_TIME = 903;
		public const ushort SMSG_PASSWORD_PROTECT_TYPE = 904;
	}

	[DataContract]
	public class CMSG_AuthLogonPermit
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string Account { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public string PasswordMd5 { get; set; }
	}

	[DataContract]
	public class SMSG_Lock_For_Safe_Time
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint Time { get; set; }
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

		[DataMember(Order = 4, IsRequired = false)]
		public byte[] PpcCoordinate { get; set; }
	}

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
	public class CMSG_AuthLogonChallenge
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string GameName { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public uint Version1 { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public uint Version2 { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public uint Version3 { get; set; }

		[DataMember(Order = 5, IsRequired = true)]
		public uint Build { get; set; }

		[DataMember(Order = 6, IsRequired = true)]
		public uint Platform { get; set; }

		[DataMember(Order = 7, IsRequired = true)]
		public uint OS { get; set; }

		[DataMember(Order = 8, IsRequired = true)]
		public uint Country { get; set; }

		[DataMember(Order = 9, IsRequired = true)]
		public uint TimeMapBias { get; set; }

		[DataMember(Order = 10, IsRequired = true)]
		public uint IP { get; set; }

		[DataMember(Order = 11, IsRequired = true)]
		public byte[] Password { get; set; }

		[DataMember(Order = 12, IsRequired = true)]
		public byte[] I { get; set; }
	}
}