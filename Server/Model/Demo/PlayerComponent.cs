using System.Collections.Generic;

namespace ET
{
	[ComponentOf(typeof(Scene))]
	[ChildType(typeof(Player))]
	public class PlayerComponent : Entity, IAwake, IDestroy
	{
		public readonly Dictionary<long, Player> idPlayers = new Dictionary<long, Player>();
	}
}