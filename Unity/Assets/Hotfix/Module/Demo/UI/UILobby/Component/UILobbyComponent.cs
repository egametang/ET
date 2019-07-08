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
			MapHelper.EnterMapAsync().Coroutine();
		}
		

	}
}
