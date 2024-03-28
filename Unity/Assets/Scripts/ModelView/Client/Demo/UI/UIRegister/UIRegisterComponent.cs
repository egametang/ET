/*********************************************
 * 
 * 脚本名：UIRegisterComponent.cs
 * 创建时间：2024/03/28 11:40:37
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UIRegisterComponent: Entity, IAwake 
	{
        public InputField Account;
        public Button CloseBtn;
        public Button LoginBtn;
        public InputField Password;

	}
}
