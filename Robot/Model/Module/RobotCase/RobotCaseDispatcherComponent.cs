using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class RobotCaseDispatcherComponent: Entity, IAwake, ILoad
    {
        public static RobotCaseDispatcherComponent Instance;
        
        public Dictionary<int, IRobotCase> Dictionary = new Dictionary<int, IRobotCase>();
    }
}