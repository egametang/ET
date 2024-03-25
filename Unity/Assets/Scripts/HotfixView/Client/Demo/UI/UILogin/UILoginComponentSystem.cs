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
            self.Account = rc.Get<GameObject>("Account").GetComponent<InputField>();
            self.CloseBtn = rc.Get<GameObject>("CloseBtn").GetComponent<Button>();;
			self.CloseBtn.onClick.AddListener(() => { self.OnClose();});
            self.LoginBtn = rc.Get<GameObject>("LoginBtn").GetComponent<Button>();;;
            self.LoginBtn.onClick.AddListener(() => { self.OnLogin();});
            self.Password = rc.Get<GameObject>("Password").GetComponent<InputField>();

			//图集加载测试
            var loader = self.AddComponent<LoaderComponent,string>("DefaultPackage");
            Sprite sp = loader.GetSpriteSync("Sprite","ffxiv");
            self.CloseBtn.image.sprite = sp;
            Debug.Log("sp.InstanceId="+sp.GetInstanceID());
            //UI创建测试
            var obj2 = loader.LoadAssetSync<UnityEngine.GameObject>("UIHelp");
            var uiHelp2 = UnityEngine.Object.Instantiate(obj2);
            Debug.Log("obj2.InstanceId="+obj2.GetInstanceID());
			//Close
            loader.Dispose();
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
