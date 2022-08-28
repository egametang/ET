namespace ET.Server
{
    [ChildOf(typeof(RobotCaseComponent))]
    public class RobotCase: Entity, IAwake
    {
        public ETCancellationToken CancellationToken;
        public string CommandLine;
    }
}