/*********************************************
 * 
 * 脚本名：UIRegisterComponentSystem.cs
 * 创建时间：2024/03/28 11:19:01
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(UIRegisterComponent))]
	[FriendOf(typeof(UIRegisterComponent))]
	public static partial class UIRegisterComponentSystem
	{
        [EntitySystem]
        private static void Awake(this UIRegisterComponent self)
		{
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.Account = rc.Get<GameObject>("Account").GetComponent<InputField>();
            self.CloseBtn = rc.Get<GameObject>("CloseBtn").GetComponent<Button>();
            self.CloseBtn.onClick.AddListener(() => { self.OnClose();});
            self.LoginBtn = rc.Get<GameObject>("LoginBtn").GetComponent<Button>();
            self.LoginBtn.onClick.AddListener(() => { self.OnLogin();});
            self.Password = rc.Get<GameObject>("Password").GetComponent<InputField>();

		}

        public static void OnClose(this UIRegisterComponent self)
        {
            Debug.Log("Click CloseBtn");
        }

        public static void OnLogin(this UIRegisterComponent self)
        {
            Debug.Log("Click LoginBtn");
        }

	}
}
