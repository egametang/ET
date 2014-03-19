using System;
using System.Threading.Tasks;
using BossBase;
using Helper;
using Logger;

namespace BossClient
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

		public void SendMessage<T>(ushort opcode, T message, byte channelID = 0)
		{
			this.IMessageChannel.SendMessage(opcode, message, channelID);
		}

		public async Task Login(SRP6Client srp6Client)
		{
			var smsgAuthChallenge = await this.Handle_SMSG_Auth_Challenge();

			var clientSeed = (uint)TimeHelper.EpochTimeSecond();
			byte[] digest = srp6Client.CalculateGateDigest(clientSeed, smsgAuthChallenge.Seed);

			var cmsgAuthSession = new CMSG_Auth_Session
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

			var smsgAuthResponse = await Handle_SMSG_Auth_Response();

			if (smsgAuthResponse.ErrorCode != ErrorCode.AUTH_OK)
			{
				throw new BossException(string.Format(
					"session: {0}, SMSG_Auth_Response: {1}",
					this.ID, MongoHelper.ToJson(smsgAuthResponse)));
			}

			Log.Trace("session: {0}, login gate OK!", this.ID);
		}

		public async Task<SMSG_Auth_Challenge> Handle_SMSG_Auth_Challenge()
		{
			var result = await this.IMessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] message = result.Item2;
			Log.Debug("message: {0}", message.ToHex());
			if (opcode != MessageOpcode.SMSG_AUTH_CHALLENGE)
			{
				throw new BossException(string.Format(
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
				throw new BossException(string.Format(
					"session: {0}, opcode: {1}", this.ID, opcode));
			}

			var smsgAuthResponse = ProtobufHelper.FromBytes<SMSG_Auth_Response>(message);
			return smsgAuthResponse;
		}
	}
}
