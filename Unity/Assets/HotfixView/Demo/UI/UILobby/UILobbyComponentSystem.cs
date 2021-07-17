using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public class UILobbyComponentAwakeSystem : AwakeSystem<UILobbyComponent>
    {
        public override void Awake(UILobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			
            self.enterMap = rc.Get<GameObject>("EnterMap");
            self.enterMap.GetComponent<Button>().onClick.AddListener(self.EnterMap);
            self.text = rc.Get<GameObject>("Text").GetComponent<Text>();
        }
    }
    
    public static class UILobbyComponentSystem
    {
        public static void EnterMap(this UILobbyComponent self)
        {
            MapHelper.EnterMapAsync(self.ZoneScene(), "Map").Coroutine();
        }
    }
}
