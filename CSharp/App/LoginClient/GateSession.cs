using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ENet;
using Helper;
using Log;

namespace LoginClient
{
	public class GateSession: IDisposable
	{
		public int ID { get; set; }

		public IMessageChannel IMessageChannel { get; set; }

		public GateSession(int id, IMessageChannel eNetChannel)
		{
			this.ID = id;
			this.IMessageChannel = eNetChannel;
		}

		public void Dispose()
		{
			this.IMessageChannel.Dispose();
		}

		public async void Login(SRP6Client srp6Client)
		{
			var smsgAuthChallenge = await this.Handle_SMSG_Auth_Challenge();

			var clientSeed = (uint)TimeHelper.EpochTimeSecond();
			byte[] digest = srp6Client.CalculateGateDigest(clientSeed, smsgAuthChallenge.Seed);

			var cmsgAuthSession = new CMSG_Auth_Session()
			{
				ClientBuild = 11723,
				ClientSeed = clientSeed,
				Digest = digest,
				Hd = new byte[0],
				Mac = new byte[0],
				Unk2 = 0,
				Unk3 = 0,
				Unk4 = 0,
				Username = srp6Client.Account
			};
			this.IMessageChannel.SendMessage(MessageOpcode.CMSG_AUTH_SESSION, cmsgAuthSession);

			await Handle_SMSG_Auth_Response();

			Logger.Trace("session: {0}, login gate OK!", this.ID);
		}

		public async Task<SMSG_Auth_Challenge> Handle_SMSG_Auth_Challenge()
		{
			var result = await this.IMessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;
			if (opcode != MessageOpcode.SMSG_AUTH_CHALLENGE)
			{
				throw new LoginException(string.Format(
					"session: {0}, opcode: {1}", this.ID, opcode));
			}

			var smsgAuthChallenge = ProtobufHelper.FromBytes<SMSG_Auth_Challenge>(message);
			return smsgAuthChallenge;
		}

		public async Task<SMSG_Auth_Response> Handle_SMSG_Auth_Response()
		{
			var result = await this.IMessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;

			if (opcode != MessageOpcode.SMSG_AUTH_RESPONSE)
			{
				throw new LoginException(string.Format(
					"session: {0}, opcode: {1}", this.ID, opcode));
			}

			var smsgAuthResponse = ProtobufHelper.FromBytes<SMSG_Auth_Response>(message);

			if (smsgAuthResponse.ErrorCode != 0)
			{
				throw new LoginException(string.Format(
					"session: {0}, SMSG_Auth_Response: {1}",
					this.ID, JsonHelper.ToString(smsgAuthResponse)));
			}

			return smsgAuthResponse;
		}
	}
}
