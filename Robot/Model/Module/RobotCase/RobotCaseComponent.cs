using System.Collections.Generic;

namespace ET
{
    public class RobotCaseComponent: Entity
    {
        public static RobotCaseComponent Instance;
        public Dictionary<int, RobotCase> RobotCases = new Dictionary<int, RobotCase>();
        public int N = 10000;
    }
}