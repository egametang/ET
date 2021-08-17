using System.Collections.Generic;

namespace ET
{
    public class RobotCaseDispatcherComponent: Entity
    {
        public static RobotCaseDispatcherComponent Instance;
        
        public Dictionary<int, IRobotCase> Dictionary = new Dictionary<int, IRobotCase>();
    }
}