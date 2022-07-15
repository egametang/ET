using System.Collections.Generic;

namespace ET.Server
{
    public class RobotCase: Entity, IAwake
    {
        public ETCancellationToken CancellationToken;
        public string CommandLine;
    }
}