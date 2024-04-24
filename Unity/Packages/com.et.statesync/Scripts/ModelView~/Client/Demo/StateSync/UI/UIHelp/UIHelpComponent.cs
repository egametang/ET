using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UIHelpComponent : Entity, IAwake
	{
		public Text text;
	}
}
