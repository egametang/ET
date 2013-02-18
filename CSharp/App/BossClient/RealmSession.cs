using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Helper;
using Log;

namespace BossClient
{
	public class RealmSession: IDisposable
	{
		public int ID { get; set; }

		public IMessageChannel MessageChannel { get; set; }

		public RealmSession(int id, IMessageChannel messageChannel)
		{
			this.ID = id;
			this.MessageChannel = messageChannel;
		}

		public void Dispose()
		{
			this.MessageChannel.Dispose();
		}

		public async Task<SMSG_Password_Protect_Type> Handle_SMSG_Password_Protect_Type()
		{
			var result = await this.MessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_PASSWORD_PROTECT_TYPE)
			{
				throw new BossException(string.Format(
					"session: {0}, opcode: {1}", this.ID, opcode));
			}

			var smsgPasswordProtectType = 
				ProtobufHelper.FromBytes<SMSG_Password_Protect_Type>(message);

			return smsgPasswordProtectType;
		}

		public async Task<SMSG_Auth_Logon_Challenge_Response>
			Handle_SMSG_Auth_Logon_Challenge_Response()
		{
			var result = await this.MessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_AUTH_LOGON_CHALLENGE_RESPONSE)
			{
				Logger.Trace("opcode: {0}", opcode);
				throw new BossException(string.Format(
					"session: {0}, opcode: {1}", this.ID, opcode));
			}
			
			var smsgAuthLogonChallengeResponse =
				ProtobufHelper.FromBytes<SMSG_Auth_Logon_Challenge_Response>(message);

			return smsgAuthLogonChallengeResponse;
		}

		public async Task<SMSG_Auth_Logon_Proof_M2> Handle_SMSG_Auth_Logon_Proof_M2()
		{
			var result = await this.MessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_AUTH_LOGON_PROOF_M2)
			{
				throw new BossException(string.Format(
					"session: {0}, error opcode: {1}", this.ID, opcode));
			}

			var smsgAuthLogonProofM2 = ProtobufHelper.FromBytes<SMSG_Auth_Logon_Proof_M2>(message);
			return smsgAuthLogonProofM2;
		}

		public async Task<SMSG_Realm_List> Handle_SMSG_Realm_List()
		{
			var result = await this.MessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_REALM_LIST)
			{
				throw new BossException(string.Format(
					"session: {0}, error opcode: {1}", this.ID, opcode));
			}

			var smsgRealmList = ProtobufHelper.FromBytes<SMSG_Realm_List>(message);

			return smsgRealmList;
		}

		public async Task<Tuple<string, ushort, SRP6Client>> Login(string account, string password)
		{
			byte[] passwordBytes = password.ToByteArray();
			MD5 md5 = MD5.Create();
			byte[] passwordMd5 = md5.ComputeHash(passwordBytes);
			byte[] passwordMd5Hex = passwordMd5.ToHex().ToLower().ToByteArray();

			// 发送帐号和密码MD5
			var cmsgAuthLogonPermit = new CMSG_Auth_Logon_Permit
			{ 
				Account = account.ToByteArray(),
				PasswordMd5 = passwordMd5Hex
			};
			
			Logger.Trace("session: {0}, account: {1}, password: {2}", this.ID,
				cmsgAuthLogonPermit.Account.ToStr(), cmsgAuthLogonPermit.PasswordMd5.ToHex());

			this.MessageChannel.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_PERMIT, cmsgAuthLogonPermit);

			var smsgPasswordProtectType = await this.Handle_SMSG_Password_Protect_Type();
			if (smsgPasswordProtectType.Code != 200)
			{
				throw new BossException(string.Format(
					"session: {0}, SMSG_Password_Protect_Type: {1}",
					this.ID, JsonHelper.ToString(smsgPasswordProtectType)));
			}

			// 这个消息已经没有作用,只用来保持原有的代码流程
			var cmsgAuthLogonChallenge = new CMSG_Auth_Logon_Challenge();
			this.MessageChannel.SendMessage(
				MessageOpcode.CMSG_AUTH_LOGON_CHALLENGE, cmsgAuthLogonChallenge);

			var smsgAuthLogonChallengeResponse = 
				await this.Handle_SMSG_Auth_Logon_Challenge_Response();
			if (smsgAuthLogonChallengeResponse.ErrorCode != ErrorCode.REALM_AUTH_SUCCESS)
			{
				throw new BossException(
					string.Format("session: {0}, SMSG_Auth_Logon_Challenge_Response: {1}",
					this.ID, JsonHelper.ToString(smsgAuthLogonChallengeResponse)));
			}

			Logger.Trace("session: {0}, SMSG_Auth_Logon_Challenge_Response OK", this.ID);

			// 以下是SRP6处理过程
			var n = smsgAuthLogonChallengeResponse.N.ToUBigInteger();
			var g = smsgAuthLogonChallengeResponse.G.ToUBigInteger();
			var b = smsgAuthLogonChallengeResponse.B.ToUBigInteger();
			var salt = smsgAuthLogonChallengeResponse.S.ToUBigInteger();

			var srp6Client = new SRP6Client(
				new SHA1Managed(), n, g, b, salt, account.ToByteArray(), passwordMd5Hex);

			Logger.Debug("s: {0}\nN: {1}\nG: {2}\nB: {3}\nA: {4}\nS: {5}\nK: {6}\nm: {7}\na: {8}",
				srp6Client.Salt.ToUBigIntegerArray().ToHex(),
				srp6Client.N.ToUBigIntegerArray().ToHex(), 
				srp6Client.G.ToUBigIntegerArray().ToHex(),
				srp6Client.B.ToUBigIntegerArray().ToHex(),
				srp6Client.A.ToUBigIntegerArray().ToHex(), 
				srp6Client.S.ToUBigIntegerArray().ToHex(),
				srp6Client.K.ToUBigIntegerArray().ToHex(), 
				srp6Client.M.ToUBigIntegerArray().ToHex(),
				srp6Client.SmallA.ToUBigIntegerArray().ToHex());

			var cmsgAuthLogonProof = new CMSG_Auth_Logon_Proof
			{
				A = srp6Client.A.ToUBigIntegerArray(),
				M = srp6Client.M.ToUBigIntegerArray()
			};
			this.MessageChannel.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_PROOF, cmsgAuthLogonProof);

			var smsgAuthLogonProofM2 = await this.Handle_SMSG_Auth_Logon_Proof_M2();
			if (smsgAuthLogonProofM2.ErrorCode != ErrorCode.REALM_AUTH_SUCCESS)
			{
				throw new BossException(string.Format(
					"session: {0}, SMSG_Auth_Logon_Proof_M2: {1}",
					this.ID, JsonHelper.ToString(smsgAuthLogonProofM2)));
			}

			Logger.Trace("session: {0}, SMSG_Auth_Logon_Proof_M2 OK", this.ID);

			// 请求realm list
			var cmsgRealmList = new CMSG_Realm_List();
			this.MessageChannel.SendMessage(MessageOpcode.CMSG_REALM_LIST, cmsgRealmList);
			var smsgRealmList = await this.Handle_SMSG_Realm_List();

			Logger.Trace("session: {0}, SMSG_Realm_List OK", this.ID);

			string address = smsgRealmList.GateList[0].Address.ToStr();
			string[] split = address.Split(new[] { ':' });

			if (split.Length != 2)
			{
				throw new BossException(
					string.Format("session: {0}, gate address error, address: {1}",
					this.ID, address));
			}
			string gateIP = split[0];
			ushort gatePort = UInt16.Parse(split[1]);
			return Tuple.Create(gateIP, gatePort, srp6Client);
		}
	}
}