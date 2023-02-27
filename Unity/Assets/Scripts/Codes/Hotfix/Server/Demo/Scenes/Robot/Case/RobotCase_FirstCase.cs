using System;

namespace ET.Server
{
    [Invoke(RobotCaseType.FirstCase)]
    public class RobotCase_FirstCase: ARobotCase
    {
        protected override async ETTask Run(RobotCase robotCase)
        {
            using ListComponent<Scene> robots = ListComponent<Scene>.Create();
            
            // 创建了两个机器人，生命周期是RobotCase，RobotCase_FirstCase.Run执行结束，机器人就会删除
            await robotCase.NewRobot(2, robots);

            using ListComponent<ETTask> robotsTasks = ListComponent<ETTask>.Create();
            for (int i = 0; i < 50; ++i)
            {
                robotsTasks.Add(robotCase.NewRobot(i, robots));
            }

            await ETTaskHelper.WaitAll(robotsTasks);

            foreach (Scene robotScene in robots)
            {
                M2C_TestRobotCase response = await robotScene.GetComponent<Client.SessionComponent>().Session.Call(new C2M_TestRobotCase() {N = robotScene.Zone}) as M2C_TestRobotCase;
                if (response.N != robotScene.Zone)
                {
                    // 跟预期不一致就抛异常，外层会catch住在控制台上打印
                    throw new Exception($"robot case: {RobotCaseType.FirstCase} run fail!");
                }
            }
        }
    }
}