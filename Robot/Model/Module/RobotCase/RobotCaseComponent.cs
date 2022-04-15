using System.Collections.Generic;

namespace ET
{
    [ChildType(typeof(RobotCase))]
    public class RobotCaseComponent: Entity, IAwake, IDestroy
    {
        public static RobotCaseComponent Instance;
        public Dictionary<int, RobotCase> RobotCases = new Dictionary<int, RobotCase>();
        public int N = 10000;
    }
}