using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helper;
using Log;
using Org.BouncyCastle.Crypto.Digests;
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

		public async Task<bool> Handle_CMSG_AuthLogonPermit_Response()
		{
			var result = await this.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode == 0)
			{
				Logger.Trace("opcode == 0");
				throw new RealmException("opcode == 0");
			}

			if (opcode == MessageOpcode.SMSG_LOCK_FOR_SAFE_TIME)
			{
				var smsgLockForSafeTime = ProtobufHelper.FromBytes<SMSG_Lock_For_Safe_Time>(message);
				Logger.Trace("account lock time: {0}", smsgLockForSafeTime.Time);
				return false;
			}

			if (opcode != MessageOpcode.SMSG_PASSWORD_PROTECT_TYPE)
			{
				throw new RealmException(string.Format("error opcode: {0}", opcode));
			}

			var smsgPasswordProtectType = ProtobufHelper.FromBytes<SMSG_Password_Protect_Type>(message);
			this.realmInfo.SmsgPasswordProtectType = smsgPasswordProtectType;

			Logger.Trace("message: {0}", JsonHelper.ToString(smsgPasswordProtectType));

			if (smsgPasswordProtectType.Code != 200)
			{
				return false;
			}

			return true;
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
					return new Tuple<ushort, byte[]>(0, new byte[] { });
				}
				totalReadSize += readSize;
			}

			int packetSize = BitConverter.ToInt32(packetBytes, 0);

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
					return new Tuple<ushort, byte[]>(0, new byte[] { });
				}
				totalReadSize += readSize;
			}
			ushort opcode = BitConverter.ToUInt16(contentBytes, 0);
			var messageBytes = new byte[needReadSize - sizeof (ushort)];
			Array.Copy(contentBytes, sizeof (ushort), messageBytes, 0, messageBytes.Length);

			return new Tuple<ushort, byte[]>(opcode, messageBytes);
		}

		public async Task<bool> Login(string account, string password)
		{
			byte[] passwordBytes = password.ToByteArray();
			var digest = new MD5Digest();
			var passwordMd5 = new byte[digest.GetDigestSize()];

			digest.BlockUpdate(passwordBytes, 0, passwordBytes.Length);
			digest.DoFinal(passwordMd5, 0);

			var cmsgAuthLogonPermit = new CMSG_AuthLogonPermit
			{ 
				Account = account, 
				PasswordMd5 = Hex.ToHexString(passwordMd5) 
			};

			this.SendMessage(MessageOpcode.CMSG_AUTHLOGONPERMIT, cmsgAuthLogonPermit);

			bool result = await this.Handle_CMSG_AuthLogonPermit_Response();

			if (result == false)
			{
				return false;
			}

			var cmsgAuthLogonChallenge = new CMSG_AuthLogonChallenge { };

			return true;
		}
	}
}