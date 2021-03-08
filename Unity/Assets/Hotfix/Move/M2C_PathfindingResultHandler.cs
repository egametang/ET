namespace ET
{
	[MessageHandler]
	public class M2C_PathfindingResultHandler : AMHandler<M2C_PathfindingResult>
	{
		protected override async ETVoid Run(Session session, M2C_PathfindingResult message)
		{
			Unit unit = session.Domain.GetComponent<UnitComponent>().Get(message.Id);
			unit.GameObject.transform.position = unit.Position = new UnityEngine.Vector3(message.X, message.Y, message.Z);
			await ETTask.CompletedTask;
		}
	}
}
