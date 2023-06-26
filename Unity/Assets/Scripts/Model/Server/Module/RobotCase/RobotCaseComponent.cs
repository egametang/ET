using System;
using System.Collections.Generic;

namespace ET.Server
{
    
    [ComponentOf(typeof(Fiber))]
    public class RobotCaseComponent: Entity, IAwake, IDestroy
    {
        public Dictionary<int, RobotCase> RobotCases = new Dictionary<int, RobotCase>();
        public int N = 10000;
    }
}