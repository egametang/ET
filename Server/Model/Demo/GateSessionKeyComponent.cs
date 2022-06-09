using System.Collections.Generic;

namespace ET
{
	[ComponentOf(typeof(Scene))]
	public class GateSessionKeyComponent : Entity, IAwake
	{
		public readonly Dictionary<long, string> sessionKey = new Dictionary<long, string>();
	}
}
