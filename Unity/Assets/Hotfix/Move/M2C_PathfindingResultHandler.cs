namespace ET
{
	[MessageHandler]
	public class M2C_PathfindingResultHandler : AMHandler<M2C_PathfindingResult>
	{
		protected override async ETTask Run(Session session, M2C_PathfindingResult message)
		{
			Unit unit = session.Domain.GetComponent<UnitComponent>().Get(message.Id);
			
			
			await ETTask.CompletedTask;
		}
	}
}
