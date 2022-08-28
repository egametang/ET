namespace ET.Server
{
    // 这里为什么能定义class呢？因为这里只有逻辑，热重载后新的handler替换旧的，仍然没有问题
    [EnableClass]
    public abstract class ARobotCase: ACallbackHandler<RobotCallbackArgs, ETTask>
    {
        protected abstract ETTask Run(RobotCase robotCase);

        public override async ETTask Handle(RobotCallbackArgs a)
        {
            using RobotCase robotCase = await RobotCaseComponent.Instance.New();
            
            try
            {
                await this.Run(robotCase);
            }
            catch (System.Exception e)
            {
                Log.Error($"{robotCase.DomainZone()} {e}");
                RobotLog.Console($"RobotCase Error {a.Id}:\n\t{e}");
            }
        }
    }
}