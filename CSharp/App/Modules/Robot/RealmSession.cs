using System;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Helper;
using Log;
using Robot.Protos;

namespace Robot
{
	public class RealmSession: IDisposable
	{
		private readonly TcpClient tcpClient = new TcpClient();
		private NetworkStream networkStream;
		private readonly RealmInfo realmInfo = new RealmInfo();

		public void Dispose()
		{
			this.tcpClient.Close();
			this.networkStream.Dispose();
		}

		public async void SendMessage<T>(ushort opcode, T message)
		{
			byte[] protoBytes = ProtobufHelper.ToBytes(message);
			var neworkBytes = new byte[sizeof (int) + sizeof (ushort) + protoBytes.Length];

			int totalSize = sizeof (ushort) + protoBytes.Length;

			var totalSizeBytes = BitConverter.GetBytes(totalSize);
			totalSizeBytes.CopyTo(neworkBytes, 0);

			var opcodeBytes = BitConverter.GetBytes(opcode);
			opcodeBytes.CopyTo(neworkBytes, sizeof (int));

			protoBytes.CopyTo(neworkBytes, sizeof (int) + sizeof (ushort));

			await this.networkStream.WriteAsync(neworkBytes, 0, neworkBytes.Length);
		}

		public async Task<SMSG_Password_Protect_Type> Handle_CMSG_AuthLogonPermit_Response()
		{
			var result = await this.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_PASSWORD_PROTECT_TYPE)
			{
				Logger.Trace("opcode: {0}", opcode);
				throw new RealmException(string.Format("error opcode: {0}", opcode));
			}

			var smsgPasswordProtectType = 
				ProtobufHelper.FromBytes<SMSG_Password_Protect_Type>(message);

			Logger.Trace("message: {0}", JsonHelper.ToString(smsgPasswordProtectType));

			if (smsgPasswordProtectType.Code != 200)
			{
				throw new RealmException(string.Format(
					"SMSG_Lock_For_Safe_Time: {0}", 
					JsonHelper.ToString(smsgPasswordProtectType)));
			}

			return smsgPasswordProtectType;
		}

		public async Task<SMSG_Auth_Logon_Challenge_Response>
			Handle_SMSG_Auth_Logon_Challenge_Response()
		{
			var result = await this.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_AUTH_LOGON_CHALLENGE_RESPONSE)
			{
				Logger.Trace("opcode: {0}", opcode);
			}

			var smsgAuthLogonChallengeResponse =
				ProtobufHelper.FromBytes<SMSG_Auth_Logon_Challenge_Response>(message);

			if (smsgAuthLogonChallengeResponse.ErrorCode != ErrorCode.REALM_AUTH_SUCCESS)
			{
				Logger.Trace("error code: {0}", smsgAuthLogonChallengeResponse.ErrorCode);
				throw new RealmException(
					string.Format("SMSG_Auth_Logon_Challenge_Response ErrorCode: {0}", 
					JsonHelper.ToString(smsgAuthLogonChallengeResponse)));
			}

			return smsgAuthLogonChallengeResponse;
		}

		public async Task<Tuple<ushort, byte[]>> RecvMessage()
		{
			int totalReadSize = 0;
			int needReadSize = sizeof (int);
			var packetBytes = new byte[needReadSize];
			while (totalReadSize != needReadSize)
			{
				int readSize = await this.networkStream.ReadAsync(
					packetBytes, totalReadSize, packetBytes.Length);
				if (readSize == 0)
				{
					throw new RealmException("connection closed!");
				}
				totalReadSize += readSize;
			}

			int packetSize = BitConverter.ToInt32(packetBytes, 0);

			Logger.Debug("packet size: {0}", packetSize);

			// 读opcode和message
			totalReadSize = 0;
			needReadSize = packetSize;
			var contentBytes = new byte[needReadSize];
			while (totalReadSize != needReadSize)
			{
				int readSize = await this.networkStream.ReadAsync(
					contentBytes, totalReadSize, contentBytes.Length);
				if (readSize == 0)
				{
					throw new RealmException("connection closed!");
				}
				totalReadSize += readSize;
			}

			ushort opcode = BitConverter.ToUInt16(contentBytes, 0);

			Logger.Debug("opcode: {0}", opcode);

			var messageBytes = new byte[needReadSize - sizeof (ushort)];
			Array.Copy(contentBytes, sizeof (ushort), messageBytes, 0, messageBytes.Length);

			return new Tuple<ushort, byte[]>(opcode, messageBytes);
		}

		public async Task ConnectAsync(string hostName, ushort port)
		{
			await this.tcpClient.ConnectAsync(hostName, port);
			this.networkStream = this.tcpClient.GetStream();
		}

		public async void Login(string account, string password)
		{
			byte[] passwordBytes = password.ToByteArray();
			MD5 md5 = MD5.Create();
			byte[] passwordMd5 = md5.ComputeHash(passwordBytes);

			// 发送帐号和密码MD5
			var cmsgAuthLogonPermit = new CMSG_Auth_Logon_Permit
			{ 
				Account = account.ToByteArray(),
				PasswordMd5 = passwordMd5.ToHex().ToLower().ToByteArray()
			};

			Logger.Trace("account: {0}, password: {1}", 
				cmsgAuthLogonPermit.Account, cmsgAuthLogonPermit.PasswordMd5.ToStr());

			this.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_PERMIT, cmsgAuthLogonPermit);
			await this.Handle_CMSG_AuthLogonPermit_Response();

			// 这个消息已经没有作用,只用来保持原有的代码流程
			var cmsgAuthLogonChallenge = new CMSG_Auth_Logon_Challenge();
			this.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_CHALLENGE, cmsgAuthLogonChallenge);
			var smsgAuthLogonChallengeResponse = 
				await this.Handle_SMSG_Auth_Logon_Challenge_Response();

			// 以下是SRP6处理过程
			var n = smsgAuthLogonChallengeResponse.N.ToUnsignedBigInteger();
			var g = smsgAuthLogonChallengeResponse.G.ToUnsignedBigInteger();
			var s = smsgAuthLogonChallengeResponse.S.ToUnsignedBigInteger();
			var b = smsgAuthLogonChallengeResponse.B.ToUnsignedBigInteger();
			string identity = account + ":" + password;

			var srp6Client = new SRP6Client(new SHA1Managed(), n, g, b, s, identity, password);

			Logger.Debug("N: {0}\nG: {1}\ns: {2}\nB: {3}\nA: {4}\nS: {5}\nK: {6}\nm: {7}",
				srp6Client.N.ToTrimByteArray().ToHex(), srp6Client.G.ToTrimByteArray().ToHex(),
				srp6Client.S.ToTrimByteArray().ToHex(), srp6Client.B.ToTrimByteArray().ToHex(),
				srp6Client.A.ToTrimByteArray().ToHex(), srp6Client.S.ToTrimByteArray().ToHex(),
				srp6Client.K.ToTrimByteArray().ToHex(), srp6Client.M.ToHex());

			var cmsgAuthLogonProof = new CMSG_Auth_Logon_Proof
			{
				A = srp6Client.A.ToTrimByteArray(),
				M = srp6Client.M
			};
			this.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_PROOF, cmsgAuthLogonProof);
		}
	}
}