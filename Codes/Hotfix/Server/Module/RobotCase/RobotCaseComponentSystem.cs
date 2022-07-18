using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(RobotCaseComponent))]
    public static class RobotCaseComponentSystem
    {
        [ObjectSystem]
        public class RobotCaseComponentAwakeSystem: AwakeSystem<RobotCaseComponent>
        {
            protected override void Awake(RobotCaseComponent self)
            {
                RobotCaseComponent.Instance = self;
            }
        }

        [ObjectSystem]
        public class RobotCaseComponentDestroySystem: DestroySystem<RobotCaseComponent>
        {
            protected override void Destroy(RobotCaseComponent self)
            {
                RobotCaseComponent.Instance = null;
            }
        }
        
        public static int GetN(this RobotCaseComponent self)
        {
            return ++self.N;
        }
        
        public static async ETTask<RobotCase> New(this RobotCaseComponent self)
        {
            await ETTask.CompletedTask;
            RobotCase robotCase = self.AddChild<RobotCase>();
            return robotCase;
        }
    }
}