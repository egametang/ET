using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Helper;
using Log;
using Org.BouncyCastle.Utilities.Encoders;
using ProtoBuf;
using Robot.Protos;
using Helper;
using Org.BouncyCastle.Crypto.Digests;

namespace Robot
{
	public class RealmSession: IDisposable
	{
		private readonly NetworkStream networkStream;

		public RealmSession(string host, ushort port)
		{
			Socket socket = ConnectSocket(host, port);
			networkStream = new NetworkStream(socket);
		}

		public void Dispose()
		{
			networkStream.Dispose();
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
			byte[] protoBytes = ProtobufHelper.ProtoToBytes(message);
			var neworkBytes = new byte[sizeof(int) + sizeof(ushort) + protoBytes.Length];

			int totalSize = sizeof(ushort) + protoBytes.Length;
			
			var totalSizeBytes = BitConverter.GetBytes(totalSize);
			totalSizeBytes.CopyTo(neworkBytes, 0);

			var opcodeBytes = BitConverter.GetBytes(opcode);
			opcodeBytes.CopyTo(neworkBytes, sizeof(int));
			
			protoBytes.CopyTo(neworkBytes, sizeof(int) + sizeof(ushort));

			await networkStream.WriteAsync(neworkBytes, 0, neworkBytes.Length);
		}

		public void Login(string account, string password)
		{
			try
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

				Logger.Debug("password: {0}", cmsgAuthLogonPermit.PasswordMd5);

				SendMessage(MessageOpcode.CMSG_AUTHLOGONPERMIT, cmsgAuthLogonPermit);
			}
			catch (SocketException e)
			{
				Logger.Debug("socket exception, error: {}", e.ErrorCode);
			}
		}
	}
}
