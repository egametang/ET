using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(RobotCaseComponent))]
    public static partial class RobotCaseComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RobotCaseComponent self)
        {
            RobotCaseComponent.Instance = self;
        }

        [EntitySystem]
        private static void Destroy(this RobotCaseComponent self)
        {
            RobotCaseComponent.Instance = null;
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