using System.Collections.Generic;

namespace ET.Server
{
    [ChildOf(typeof(RobotCaseComponent))]
    public class RobotCase: Entity, IAwake, IDestroy
    {
        public ETCancellationToken CancellationToken;
        public string CommandLine;
        public HashSet<long> Scenes { get; } = new HashSet<long>();
    }
}