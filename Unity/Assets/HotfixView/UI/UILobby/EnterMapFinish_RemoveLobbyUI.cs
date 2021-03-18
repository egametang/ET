﻿namespace ET
{
	public class EnterMapFinish_RemoveLobbyUI: AEvent<EventType.EnterMapFinish>
	{
		protected override async ETTask Run(EventType.EnterMapFinish args)
		{
			// 加载场景资源
			await ResourcesComponent.Instance.LoadBundleAsync("map.unity3d");
			// 切换到map场景
			using (SceneChangeComponent sceneChangeComponent = Game.Scene.AddComponent<SceneChangeComponent>())
			{
				await sceneChangeComponent.ChangeSceneAsync("Map");
			}
            args.ZoneScene.AddComponent<OperaComponent>();
            await UIHelper.Remove(args.ZoneScene, UIType.UILobby);
		}
	}
}
