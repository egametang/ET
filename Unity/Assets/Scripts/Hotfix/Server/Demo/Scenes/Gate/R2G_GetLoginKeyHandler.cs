using System;


namespace ET.Server
{
	[ActorMessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : ActorMessageHandler<Fiber, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Fiber fiber, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			fiber.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			response.Key = key;
			response.GateId = fiber.Id;
			await ETTask.CompletedTask;
		}
	}
}