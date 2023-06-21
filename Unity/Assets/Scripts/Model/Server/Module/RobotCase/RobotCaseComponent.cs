using System;
using System.Collections.Generic;

namespace ET.Server
{
    
    [ComponentOf(typeof(VProcess))]
    public class RobotCaseComponent: SingletonEntity<RobotCaseComponent>, IAwake, IDestroy
    {
        public Dictionary<int, RobotCase> RobotCases = new Dictionary<int, RobotCase>();
        public int N = 10000;
    }
}