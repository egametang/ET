using System.Collections;
using System.Collections.Generic;
using System;
using ET.Client;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[FriendOf(typeof(DlgLogin))]
	public static  class DlgLoginSystem
	{

		public static void RegisterUIEvent(this DlgLogin self)
		{
			self.View.E_LoginBtnButton.onClick.AddListener(() => { self.OnLoginClickHandler();});
		}

		public static void ShowWindow(this DlgLogin self, Entity contextData = null)
		{
		}

		public static void OnLoginClickHandler(this DlgLogin self)
		{
			LoginHelper.Login(
				self.DomainScene(), 
				self.View.E_AccountInputField.text, 
				self.View.E_PasswordInputField.text).Coroutine();
		}

	}
}
