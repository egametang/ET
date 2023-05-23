using System.Collections;
using System.Collections.Generic;
using System;
using ET.Client;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[FriendOf(typeof(DlgLobby))]
	public static  class DlgLobbySystem
	{

		public static void RegisterUIEvent(this DlgLobby self)
		{
			self.View.E_EnterMapButton.AddListener(()=>
			{
				self.OnEnterMapClickHandler().Coroutine();
			});
		}

		public static void ShowWindow(this DlgLobby self, Entity contextData = null)
		{
			
		}

		public static async ETTask OnEnterMapClickHandler(this DlgLobby self)
		{
			await EnterMapHelper.EnterMapAsync(self.ClientScene());
		}

	}
}
