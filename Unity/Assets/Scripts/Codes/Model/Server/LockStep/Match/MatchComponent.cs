using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class MatchComponent: Entity, IAwake
    {
        public List<(long, long)> waitMatchPlayers = new List<(long, long)>();
    }

}