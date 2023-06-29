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
        [ThreadStatic]
        [StaticField]
        private static Fiber instance;

        public static Fiber Instance
        {
            get
            {
                return instance;
            }
        }

        public int Id;

        public Scene Root { get; }

        public Address Address
        {
            get
            {
                return new Address(this.Process, this.Id);
            }
        }
        
        public int Process { get; private set; }
        
        public EntitySystem EntitySystem { get; }
        public TimeInfo TimeInfo { get; }
        public IdGenerater IdGenerater { get; }
        public Mailboxes Mailboxes { get; }

        public bool IsRuning;
        
        public Fiber(int id, int process, SceneType sceneType)
        {
            this.Id = id;
            this.Process = process;
            this.EntitySystem = new EntitySystem();
            this.TimeInfo = new TimeInfo();
            this.IdGenerater = new IdGenerater(process, this.TimeInfo);
            this.Mailboxes = new Mailboxes();

            this.Root = new Scene(id, 1, 0, sceneType, "");
        }

        public void Update()
        {
            instance = this;
            
            this.TimeInfo.Update();
            
            this.EntitySystem.Update();
        }
        
        public void LateUpdate()
        {
            instance = this;
            
            this.EntitySystem.LateUpdate();

            FrameFinishUpdate();
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
            this.IsRuning = false;

            instance = null;
        }
    }
}