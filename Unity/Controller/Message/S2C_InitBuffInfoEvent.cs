using Base;
using Model;

namespace Controller
{
	[Message(MessageType.Client, Opcode.S2C_InitBuffInfo)]
	public class S2C_InitBuffInfoEvent: AMEvent<S2C_InitBuffInfo>
	{
		public override void Run(Entity scene, S2C_InitBuffInfo buffInfo)
		{
			Log.Info(MongoHelper.ToJson(buffInfo));
		}
	}
}