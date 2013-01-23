using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

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
		public string Account
		{
			get;
			set;
		}
		[DataMember(Order = 2, IsRequired = true)]
		public string PasswordMd5
		{
			get;
			set;
		}
	}


	[DataContract]
	public class SMSG_Auth_Challenge
	{
		[DataMember(Order = 1, IsRequired = true)]
		public uint Num
		{
			get;
			set;
		}
		[DataMember(Order = 2, IsRequired = true)]
		public uint Seed
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		public List<uint> Random
		{
			get;
			set;
		}
	}
}
