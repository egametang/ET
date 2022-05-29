using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[ComponentOf(typeof(UI))]
	public class UIHelpComponent : Entity, IAwake
	{
		public Text text;
	}
}
