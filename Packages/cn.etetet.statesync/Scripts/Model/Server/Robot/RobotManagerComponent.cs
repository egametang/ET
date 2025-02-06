using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RobotManagerComponent: Entity, IAwake, IDestroy
    {
        public HashSet<int> robots = new();
    }
}