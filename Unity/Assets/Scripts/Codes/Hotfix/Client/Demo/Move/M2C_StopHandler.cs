using Unity.Mathematics;

namespace ET.Client
{
	[MessageHandler(SceneType.Client)]
	public class M2C_StopHandler : AMHandler<M2C_Stop>
	{
		protected override async ETTask Run(Session session, M2C_Stop message)
		{
			Unit unit = session.DomainScene().CurrentScene().GetComponent<UnitComponent>().Get(message.Id);
			if (unit == null)
			{
				return;
			}

			float3 pos = new float3(message.X, message.Y, message.Z);
			quaternion rotation = new quaternion(message.A, message.B, message.C, message.W);

			MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
			moveComponent.Stop();
			unit.Position = pos;
			unit.Rotation = rotation;
			unit.GetComponent<ObjectWait>()?.Notify(new Wait_UnitStop() {Error = message.Error});
			await ETTask.CompletedTask;
		}
	}
}
