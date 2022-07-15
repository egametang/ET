using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
	[ChildType(typeof(Player))]
	[ComponentOf(typeof(Scene))]
	public class PlayerComponent : Entity, IAwake, IDestroy
	{
		public readonly Dictionary<long, Player> idPlayers = new Dictionary<long, Player>();
	}
}