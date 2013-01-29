using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Helper;
using Log;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Robot.Protos;

namespace Robot
{
	public class RealmSession: IDisposable
	{
		private readonly NetworkStream networkStream;
		private readonly RealmInfo realmInfo = new RealmInfo();

		public RealmSession(string host, ushort port)
		{
			Socket socket = ConnectSocket(host, port);
			this.networkStream = new NetworkStream(socket);
		}

		public void Dispose()
		{
			this.networkStream.Dispose();
		}

		public static Socket ConnectSocket(string host, ushort port)
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(host);

			foreach (IPAddress address in hostEntry.AddressList)
			{
				var ipe = new IPEndPoint(address, port);
				var tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				tempSocket.Connect(ipe);

				if (!tempSocket.Connected)
				{
					continue;
				}

				return tempSocket;
			}
			Logger.Debug("socket is null, address: {0}:{1}", host, port);
			throw new SocketException(10000);
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

		public async void Login(string account, string password)
		{
			byte[] passwordBytes = password.ToByteArray();
			var digest = new MD5Digest();
			var passwordMd5 = new byte[digest.GetDigestSize()];

			digest.BlockUpdate(passwordBytes, 0, passwordBytes.Length);
			digest.DoFinal(passwordMd5, 0);

			// 发送帐号和密码MD5
			var cmsgAuthLogonPermit = new CMSG_Auth_Logon_Permit
			{ 
				Account = account, 
				PasswordMd5 = Hex.ToHexString(passwordMd5) 
			};
			this.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_PERMIT, cmsgAuthLogonPermit);
			await this.Handle_CMSG_AuthLogonPermit_Response();

			// 这个消息已经没有作用,只用来保持原有的代码流程
			var cmsgAuthLogonChallenge = new CMSG_Auth_Logon_Challenge();
			this.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_CHALLENGE, cmsgAuthLogonChallenge);
			var smsgAuthLogonChallengeResponse = 
				await this.Handle_SMSG_Auth_Logon_Challenge_Response();

			// 以下是SRP6处理过程
			var random = new SecureRandom();
			var srp6Client = new Srp6Client();
			var n = new BigInteger(1, smsgAuthLogonChallengeResponse.N);
			var g = new BigInteger(1, smsgAuthLogonChallengeResponse.G);
			var s = new BigInteger(1, smsgAuthLogonChallengeResponse.S);
			var b = new BigInteger(1, smsgAuthLogonChallengeResponse.B);
			srp6Client.Init(n, g, new Sha1Digest(), random);
			BigInteger a = srp6Client.GenerateClientCredentials(
				s.ToByteArray(), account.ToByteArray(), password.ToByteArray());
			BigInteger clientS = srp6Client.CalculateSecret(b);

			Logger.Debug("N: {0}\nG: {1}, s: {2}, B: {3}, A: {4}, S: {5}",
				smsgAuthLogonChallengeResponse.N.ToHex(), smsgAuthLogonChallengeResponse.G.ToHex(),
				smsgAuthLogonChallengeResponse.S.ToHex(), smsgAuthLogonChallengeResponse.B.ToHex(),
				a.ToByteArray().ToHex(), clientS.ToByteArray().ToHex());

			var sha1Managed = new SHA1Managed();
			byte[] k = SRP6Helper.SRP6ClientCalcK(sha1Managed, clientS.ToByteArray());
			Logger.Debug("K: {0}", k.ToHex());
			byte[] m = SRP6Helper.SRP6ClientM1(
				sha1Managed, account.ToByteArray(), n.ToByteArray(), g.ToByteArray(), 
				s.ToByteArray(), a.ToByteArray(), b.ToByteArray(), k);
			Logger.Debug("M: {0}, size: {1}", m.ToHex(), m.Length);

			var cmsgAuthLogonProof = new CMSG_Auth_Logon_Proof
			{
				A = a.ToByteArray(),
				M = m
			};
			this.SendMessage(MessageOpcode.CMSG_AUTH_LOGON_PROOF, cmsgAuthLogonProof);
		}
	}
}