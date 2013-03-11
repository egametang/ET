using System;
using System.Threading.Tasks;
using BossBase;
using DataCenter;
using Helper;
using Log;

namespace BossCommand
{
	public abstract class ABossCommand
	{
		protected IMessageChannel IMessageChannel { get; set; }
		protected DataCenterEntities Entities { get; set; }


		protected void SendMessage(CMSG_Boss_Gm cmsgBossGm)
		{
			this.IMessageChannel.SendMessage(MessageOpcode.CMSG_BOSS_GM, cmsgBossGm);
		}

		protected async Task<T> RecvMessage<T>()
		{
			var result = await this.IMessageChannel.RecvMessage();
			ushort opcode = result.Item1;
			byte[] content = result.Item2;

			try
			{
				var message = ProtobufHelper.FromBytes<T>(content);
				return message;
			}
			catch (Exception)
			{
				Logger.Trace("parse message fail, opcode: {0}", opcode);
				throw;
			}
		}

		protected ABossCommand(IMessageChannel iMessageChannel, DataCenterEntities entities)
		{
			this.IMessageChannel = iMessageChannel;
			this.Entities = entities;
		}

		public virtual Task<object> DoAsync()
		{
			throw new NotImplementedException();
		}

		public virtual object Do()
		{
			throw new NotImplementedException();
		}

		public void Undo()
		{
			throw new NotImplementedException();
		}
	}
}
