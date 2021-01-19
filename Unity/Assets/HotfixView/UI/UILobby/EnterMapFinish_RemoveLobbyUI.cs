namespace ET
{
	public class EnterMapFinish_RemoveLobbyUI: AEvent<EventType.EnterMapFinish>
	{
		public override async ETTask Run(EventType.EnterMapFinish args)
		{
			// 加载场景资源
			await Game.Scene.GetComponent<ResourcesComponent>().LoadBundleAsync("map.unity3d");
			// 切换到map场景
			using (SceneChangeComponent sceneChangeComponent = Game.Scene.AddComponent<SceneChangeComponent>())
			{
				await sceneChangeComponent.ChangeSceneAsync("Map");
			}
            Game.Scene.Get(1).AddComponent<OperaComponent>();
            await UIHelper.Remove(args.ZoneScene, UIType.UILobby);
		}
	}
}
