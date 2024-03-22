/*********************************************
 * 
 * 脚本名：UILoginComponentSystem.cs
 * 创建时间：2024/03/22 18:09:44
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(UILoginComponent))]
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
        [EntitySystem]
        private static void Awake(this UILoginComponent self)
		{
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.Account = rc.Get<InputField>("Account");
            self.CloseBtn = rc.Get<Button>("CloseBtn");
            self.CloseBtn.onClick.AddListener(() => { self.OnClose();});
            self.LoginBtn = rc.Get<Button>("LoginBtn");
            self.LoginBtn.onClick.AddListener(() => { self.OnLogin();});
            self.Password = rc.Get<InputField>("Password");

		}
        
		public static void OnLogin(this UILoginComponent self)
		{
			LoginHelper.Login(
				self.Root(), 
				self.Account.text, 
				self.Password.text).Coroutine();
		}

        public static void OnClose(this UILoginComponent self)
        {

        }
	}
}
