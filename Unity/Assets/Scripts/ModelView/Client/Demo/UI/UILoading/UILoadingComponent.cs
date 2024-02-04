using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoadingComponent : Entity, IAwake
	{
		public Text text;
	}
}
