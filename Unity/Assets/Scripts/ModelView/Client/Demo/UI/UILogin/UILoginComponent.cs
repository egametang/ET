/*********************************************
 * 
 * 脚本名：UILoginComponent.cs
 * 创建时间：2024/03/26 16:24:17
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public partial class UILoginComponent: Entity, IAwake , IUpdate
	{
		public InputField Account;
		public Button CloseBtn;
		public Button LoginBtn;
		public InputField Password;
		public Image CloseBtnRedPoint;
	}

	public partial class UILoginComponent
	{
		public long timerId;
	}
}
