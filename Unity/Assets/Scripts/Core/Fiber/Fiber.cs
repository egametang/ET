using System;
using System.Collections.Generic;

namespace ET
{
    public static class FiberHelper
    {
        public static ActorId GetActorId(this Entity self)
        {
            Fiber root = self.Fiber();
            return new ActorId(root.Process, root.Id, self.InstanceId);
        }
    }
    
    public class Fiber: IDisposable
    {
        public bool IsDisposed;
        
        public int Id;

        public int Zone;

        public Scene Root { get; }

        public Address Address
        {
            get
            {
                return new Address(this.Process, this.Id);
            }
        }
        
        public int Process { get; }
        
        public EntitySystem EntitySystem { get; }
        public TimeInfo TimeInfo { get; }
        public IdGenerater IdGenerater { get; }
        public Mailboxes Mailboxes { get; }

        public bool IsRuning;
        
        public Fiber(int id, int process, int zone, SceneType sceneType, string name)
        {
            this.Id = id;
            this.Process = process;
            this.Zone = zone;
            this.EntitySystem = new EntitySystem();
            this.TimeInfo = new TimeInfo();
            this.IdGenerater = new IdGenerater(process, this.TimeInfo);
            this.Mailboxes = new Mailboxes();
            this.Root = new Scene(this, id, 1, sceneType, name);
        }

        public void Update()
        {
            this.IsRuning = true;
            //this.FiberTaskScheduler.Update();
            this.TimeInfo.Update();
            this.EntitySystem.Update();
        }
        
        public void LateUpdate()
        {
            this.EntitySystem.LateUpdate();
            FrameFinishUpdate();

            this.IsRuning = false;
        }

        public async ETTask WaitFrameFinish()
        {
            await ETTask.CompletedTask;
        }

        private void FrameFinishUpdate()
        {
        }

        public void Dispose()
        {
            this.Root.Dispose();
        }
    }
}