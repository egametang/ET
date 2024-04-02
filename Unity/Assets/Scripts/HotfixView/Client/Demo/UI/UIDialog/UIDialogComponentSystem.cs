/*********************************************
 * 
 * 脚本名：UIDialogComponentSystem.cs
 * 创建时间：2024/04/01 11:51:43
 *********************************************/

using NativeCollection.UnsafeType;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(UIDialogComponent))]
	[FriendOf(typeof(UIDialogComponent))]
	public static partial class UIDialogComponentSystem
	{
        [EntitySystem]
        private static void Awake(this UIDialogComponent self)
		{
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

		}

		public static async ETTask Chat(this UIDialogComponent self,int chat)
		{
			await ETTask.CompletedTask;
		}
	}
}
