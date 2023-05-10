using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(UILoginComponent))]
	public static class UILSLoginComponentSystem
	{
		[ObjectSystem]
		public class UILSLoginComponentAwakeSystem : AwakeSystem<UILSLoginComponent>
		{
			protected override void Awake(UILSLoginComponent self)
			{
				ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
				self.loginBtn = rc.Get<GameObject>("LoginBtn");
				
				self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnLogin(); });
				self.account = rc.Get<GameObject>("Account");
				self.password = rc.Get<GameObject>("Password");
			}
		}

		
		public static void OnLogin(this UILSLoginComponent self)
		{
			LoginHelper.Login(
				self.DomainScene(), 
				self.account.GetComponent<InputField>().text, 
				self.password.GetComponent<InputField>().text).Coroutine();
		}
	}
}
