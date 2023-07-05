namespace ET.Client
{
	[ActorMessageHandler(SceneType.Demo)]
	public class M2C_PathfindingResultHandler : ActorMessageHandler<Scene, M2C_PathfindingResult>
	{
		protected override async ETTask Run(Scene root, M2C_PathfindingResult message)
		{
			Unit unit = root.CurrentScene().GetComponent<UnitComponent>().Get(message.Id);

			float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);

			await unit.GetComponent<MoveComponent>().MoveToAsync(message.Points, speed);
		}
	}
}
