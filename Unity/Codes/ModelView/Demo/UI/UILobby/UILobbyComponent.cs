﻿
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[ComponentOf(typeof(UI))]
	public class UILobbyComponent : Entity, IAwake
	{
		public GameObject enterMap;
		public Text text;
	}
}
