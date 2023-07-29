﻿using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
	[ComponentOf(typeof(Scene))]
	public class PlayerComponent : Entity, IAwake, IDestroy
	{
		public Dictionary<string, Player> dictionary = new Dictionary<string, Player>();
	}
}