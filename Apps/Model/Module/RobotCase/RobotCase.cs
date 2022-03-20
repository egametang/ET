using System.Collections.Generic;

namespace ET
{
    public class RobotCase: Entity, IAwake
    {
        public ETCancellationToken CancellationToken;
        public string CommandLine;
    }
}