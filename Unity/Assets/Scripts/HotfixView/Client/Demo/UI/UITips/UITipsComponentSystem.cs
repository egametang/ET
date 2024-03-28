/*********************************************
 * 
 * 脚本名：UITipsComponentSystem.cs
 * 创建时间：2024/03/28 14:56:53
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(UITipsComponent))]
	[FriendOf(typeof(UITipsComponent))]
	public static partial class UITipsComponentSystem
	{
        [EntitySystem]
        private static void Awake(this UITipsComponent self)
		{
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

		}

		public static async ETTask Log(this UITipsComponent self,string log = "")
		{
			Debug.Log(log);
			await ETTask.CompletedTask;
		}

	}
}
