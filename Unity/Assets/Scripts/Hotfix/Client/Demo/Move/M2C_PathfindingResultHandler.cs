namespace ET.Client
{
	[MessageHandler(SceneType.Demo)]
	public class M2C_PathfindingResultHandler : MessageHandler<M2C_PathfindingResult>
	{
		protected override async ETTask Run(Session session, M2C_PathfindingResult message)
		{
			Unit unit = session.Scene().CurrentScene().GetComponent<UnitComponent>().Get(message.Id);

			float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);

			await unit.GetComponent<MoveComponent>().MoveToAsync(message.Points, speed);
		}
	}
}
