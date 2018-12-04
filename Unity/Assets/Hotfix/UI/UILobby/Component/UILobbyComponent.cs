using System;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
	[ObjectSystem]
	public class UiLobbyComponentSystem : AwakeSystem<UILobbyComponent>
	{
		public override void Awake(UILobbyComponent self)
		{
			self.Awake();
		}
	}
	
	public class UILobbyComponent : Component
	{
		private GameObject enterMap;
		private Text text;

		public void Awake()
		{
			ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			
			enterMap = rc.Get<GameObject>("EnterMap");
			enterMap.GetComponent<Button>().onClick.Add(this.EnterMap);

			this.text = rc.Get<GameObject>("Text").GetComponent<Text>();
		}

		private void EnterMap()
		{
			EnterMapAsync().NoAwait();
		}
		
		private async ETVoid EnterMapAsync()
		{
			try
			{
				// 加载Unit资源
				ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
				await resourcesComponent.LoadBundleAsync($"unit.unity3d");

				// 加载场景资源
				await ETModel.Game.Scene.GetComponent<ResourcesComponent>().LoadBundleAsync("map.unity3d");
				// 切换到map场景
				using (SceneChangeComponent sceneChangeComponent = ETModel.Game.Scene.AddComponent<SceneChangeComponent>())
				{
					await sceneChangeComponent.ChangeSceneAsync(SceneType.Map);
				}
				
				G2C_EnterMap g2CEnterMap = await ETModel.SessionComponent.Instance.Session.Call(new C2G_EnterMap()) as G2C_EnterMap;
				PlayerComponent.Instance.MyPlayer.UnitId = g2CEnterMap.UnitId;
				
				Game.Scene.AddComponent<OperaComponent>();
				Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}	
		}
	}
}
