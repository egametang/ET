using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILSLoginComponent: Entity, IAwake
	{
		public GameObject account;
		public GameObject password;
		public GameObject loginBtn;
	}
}
