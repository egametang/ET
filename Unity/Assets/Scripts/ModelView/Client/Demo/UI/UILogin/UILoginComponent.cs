/*********************************************
 * 
 * 脚本名：UILoginComponent.cs
 * 创建时间：2024/03/22 18:09:44
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake 
	{
        public InputField Account;
        public Button CloseBtn;
        public Button LoginBtn;
        public InputField Password;

	}
}
