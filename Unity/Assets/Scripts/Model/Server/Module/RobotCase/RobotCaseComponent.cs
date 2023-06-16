using System;
using System.Collections.Generic;

namespace ET.Server
{
    
    [ComponentOf(typeof(RootEntity))]
    public class RobotCaseComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static RobotCaseComponent Instance;
        public Dictionary<int, RobotCase> RobotCases = new Dictionary<int, RobotCase>();
        public int N = 10000;
    }
}