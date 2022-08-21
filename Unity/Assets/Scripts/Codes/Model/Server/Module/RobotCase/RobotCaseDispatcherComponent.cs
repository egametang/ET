using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RobotCaseDispatcherComponent: Entity, IAwake, ILoad
    {
        [StaticField]
        public static RobotCaseDispatcherComponent Instance;
        
        public Dictionary<int, IRobotCase> Dictionary = new Dictionary<int, IRobotCase>();
    }
}