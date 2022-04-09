using System;
using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class RobotCaseComponentAwakeSystem: AwakeSystem<RobotCaseComponent>
    {
        public override void Awake(RobotCaseComponent self)
        {
            RobotCaseComponent.Instance = self;
        }
    }

    [ObjectSystem]
    public class RobotCaseComponentDestroySystem: DestroySystem<RobotCaseComponent>
    {
        public override void Destroy(RobotCaseComponent self)
        {
            RobotCaseComponent.Instance = null;
        }
    }
    
    [FriendClass(typeof(RobotCaseComponent))]
    public static class RobotCaseComponentSystem
    {
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