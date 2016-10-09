using Base;

namespace Controller
{
	[Message(SceneType.Game)]
	public class S2C_InitBuffInfoEvent: AMEvent<S2C_InitBuffInfo>
	{
		public override void Run(Entity scene, S2C_InitBuffInfo buffInfo)
		{
			Log.Info(MongoHelper.ToJson(buffInfo));
		}
	}
}