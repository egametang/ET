using System;


namespace ET.Server
{
	public static partial class R2G_GetLoginKeyHandler
	{
		[ActorMessageHandler(SceneType.Gate)]
		private static async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			response.Key = key;
			response.GateId = scene.Id;
			await ETTask.CompletedTask;
		}
	}
}