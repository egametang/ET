using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Client)]
    public class M2C_TestRobotCase2Handler: AMHandler<M2C_TestRobotCase2>
    {
        protected override async ETTask Run(Session session, M2C_TestRobotCase2 message)
        {
            ObjectWait objectWait = session.ClientScene().GetComponent<ObjectWait>();
            if (objectWait == null)
            {
                return;
            }
            objectWait.Notify(new RobotCase_SecondCaseWait {Error = WaitTypeError.Success, M2CTestRobotCase2 = message});
            await ETTask.CompletedTask;
        }
    }

    [Invoke(RobotCaseType.SecondCase)]
    public class RobotCase_SecondCase: ARobotCase
    {
        protected override async ETTask Run(RobotCase robotCase)
        {
            // 创建了1个机器人，生命周期是RobotCase
            Scene robotScene = await robotCase.NewRobot(1);

            ObjectWait objectWait = robotScene.GetComponent<ObjectWait>();
            robotScene.GetComponent<Client.SessionComponent>().Session.Send(new C2M_TestRobotCase2() {N = robotScene.Zone});
            RobotCase_SecondCaseWait robotCaseSecondCaseWait = await objectWait.Wait<RobotCase_SecondCaseWait>();
            if (robotCaseSecondCaseWait.M2CTestRobotCase2.N != robotScene.Zone)
            {
                throw new Exception($"robot case: {RobotCaseType.SecondCase} run fail!");
            }
        }
    }
}