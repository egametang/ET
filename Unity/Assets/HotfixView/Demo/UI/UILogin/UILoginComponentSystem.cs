using System;
using System.Net;

using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[ObjectSystem]
	public class UILoginComponentAwakeSystem : AwakeSystem<UILoginComponent>
	{
		public override void Awake(UILoginComponent self)
		{
			ReferenceCollector rc = self.GetParent<UI>().ViewGO.GetComponent<ReferenceCollector>();
			self.loginBtn = rc.Get<GameObject>("LoginBtn");
			self.loginBtn.GetComponent<Button>().onClick.Add(self.OnLogin);
			self.account = rc.Get<GameObject>("Account");
		}
	}
	
	public static class UILoginComponentSystem
	{
		public static void OnLogin(this UILoginComponent self)
		{
			LoginHelper.OnLoginAsync(self.Domain, "127.0.0.1:10002", self.account.GetComponent<InputField>().text).Coroutine();
		}
	}
}
