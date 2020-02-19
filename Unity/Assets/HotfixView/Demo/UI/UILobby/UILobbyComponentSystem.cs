using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    public class UILobbyComponentAwakeSystem : AwakeSystem<UILobbyComponent>
    {
        public override void Awake(UILobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().ViewGO.GetComponent<ReferenceCollector>();
			
            self.enterMap = rc.Get<GameObject>("EnterMap");
            self.enterMap.GetComponent<Button>().onClick.Add(self.EnterMap);
            self.text = rc.Get<GameObject>("Text").GetComponent<Text>();
        }
    }
    
    public static class UILobbyComponentSystem
    {
        public static void EnterMap(this UILobbyComponent self)
        {
            MapHelper.EnterMapAsync("Map").Coroutine();
        }
    }
}