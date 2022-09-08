using System;
using System.Threading;

namespace ET
{
    [FriendOf(typeof(NetThreadComponent))]
    public static class NetThreadComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetThreadComponent>
        {
            protected override void Awake(NetThreadComponent self)
            {
                NetThreadComponent.Instance = self;

                // 网络线程
                self.thread = new Thread(self.NetThreadUpdate);
                
                self.thread.Start();
            }
        }
        
        [ObjectSystem]
        public class LateUpdateSystem: LateUpdateSystem<NetThreadComponent>
        {
            protected override void LateUpdate(NetThreadComponent self)
            {
                self.MainThreadUpdate();
            }
        }
        
        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetThreadComponent>
        {
            protected override void Destroy(NetThreadComponent self)
            {
                NetThreadComponent.Instance = null;
                self.isStop = true;
            }
        }

        // 主线程Update
        private static void MainThreadUpdate(this NetThreadComponent self)
        {
            NetServices.Instance.UpdateInMainThread();
        }

        // 网络线程Update
        private static void NetThreadUpdate(this NetThreadComponent self)
        {
            while (!self.isStop)
            {
                NetServices.Instance.UpdateInNetThread();
                Thread.Sleep(1);
            }
        }

        public static int Add(this NetThreadComponent self, AService service)
        {
            service.Id = ++self.serviceIdGenerator;
            NetServices.Instance.AddService(service);
            return service.Id;
        }
        
        public static void Remove(this NetThreadComponent self, int serviceId)
        {
            NetServices.Instance.RemoveService(serviceId);
        }
    }
}