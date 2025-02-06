namespace ET.Server
{
    [EntitySystemOf(typeof(RobotManagerComponent))]
    [FriendOf(typeof(RobotManagerComponent))]
    public static partial class RobotManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RobotManagerComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this RobotManagerComponent self)
        {
            async ETTask Remove(int f)
            {
                await FiberManager.Instance.Remove(f);
            }
            
            foreach (int fiberId in self.robots)
            {
                Remove(fiberId).NoContext();
            }
        }

        public static async ETTask NewRobot(this RobotManagerComponent self, string account)
        {
            int robot = await FiberManager.Instance.Create(SchedulerType.ThreadPool, self.Zone(), SceneType.Robot, account);
            self.robots.Add(robot);
        }
    }
}