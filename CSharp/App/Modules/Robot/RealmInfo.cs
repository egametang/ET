using Robot.Protos;

namespace Robot
{
	public class RealmInfo
	{
		public SMSG_Password_Protect_Type SmsgPasswordProtectType { get; set; }
		public SMSG_Auth_Logon_Challenge_Response SmsgAuthLogonChallengeResponse { get; set; }
	}
}