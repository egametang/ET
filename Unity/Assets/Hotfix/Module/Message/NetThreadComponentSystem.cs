using System;
using System.Threading;

namespace ET
{
    [ObjectSystem]
    public class NetThreadComponentAwakeSystem: AwakeSystem<NetThreadComponent>
    {
        public override void Awake(NetThreadComponent self)
        {
            NetThreadComponent.Instance = self;
            
#if NET_THREAD
            self.Thread = new Thread(self.Loop);
            self.ThreadSynchronizationContext = new ThreadSynchronizationContext(self.Thread.ManagedThreadId);
            self.Thread.Start();
#else
            self.ThreadSynchronizationContext = ThreadSynchronizationContext.Instance;
#endif
        }
    }

#if !NET_THREAD
    [ObjectSystem]
    public class NetThreadComponentUpdateSystem: LateUpdateSystem<NetThreadComponent>
    {
        public override void LateUpdate(NetThreadComponent self)
        {
            foreach (AService service in self.Services)
            {
                service.Update();
            }
        }
    }
#endif
    
    [ObjectSystem]
    public class NetThreadComponentDestroySystem: DestroySystem<NetThreadComponent>
    {
        public override void Destroy(NetThreadComponent self)
        {
            self.Stop();
        }
    }
    
    public static class NetThreadComponentSystem
    {
#region 主线程

        public static void Stop(this NetThreadComponent self)
        {
#if NET_THREAD
            self.ThreadSynchronizationContext.Post(()=>{self.isRun = false;});
#endif
        }

        public static void Add(this NetThreadComponent self, AService kService)
        {
            // 这里要去下一帧添加，避免foreach错误
            self.ThreadSynchronizationContext.PostNext(() =>
            {
                if (kService.IsDispose())
                {
                    return;
                }
                self.Services.Add(kService);
            });
        }
        
        public static void Remove(this NetThreadComponent self, AService kService)
        {
            // 这里要去下一帧删除，避免foreach错误
            self.ThreadSynchronizationContext.PostNext(() =>
            {
                if (kService.IsDispose())
                {
                    return;
                }
                self.Services.Remove(kService);
            });
        }

#endregion

#if NET_THREAD
#region 网络线程
        public static void Loop(this NetThreadComponent self)
        {
            self.isRun = true;
            while (true)
            {
                try
                {
                    if (!self.isRun)
                    {
                        return;
                    }

                    self.ThreadSynchronizationContext.Update();

                    foreach (AService service in self.Services)
                    {
                        service.Update();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                
                Thread.Sleep(1);
            }
        }
#endregion
#endif
        
    }
}