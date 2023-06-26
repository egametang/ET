using System;
using System.Collections.Generic;

namespace ET
{
    public static class FiberHelper
    {
        public static ActorId GetActorId(this Entity self)
        {
            Fiber root = self.Fiber();
            return new ActorId(root.Process, (int)root.Id, self.InstanceId);
        }
    }
    
    public class Fiber: Entity, IScene, IEntitySystem, IIdGenerater
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

        public Address Address
        {
            get
            {
                return new Address(this.Process, (int)this.Id);
            }
        }
        
        public IScene Root { get; set; }
        public SceneType SceneType { get; set; }
        
        public int Process { get; private set; }
        
        public EntitySystem EntitySystem { get; }
        public TimeInfo TimeInfo { get; }
        public IdGenerater IdGenerater { get; }
        public Mailboxes Mailboxes { get; }

        public bool IsRuning;
        
        public Fiber(int id, int process, SceneType sceneType)
        {
            this.SceneType = sceneType;
            this.Id = id;
            this.Process = process;
            this.Root = this;
            this.EntitySystem = new EntitySystem();
            this.TimeInfo = new TimeInfo();
            this.IdGenerater = new IdGenerater(process, this.TimeInfo);
            this.Mailboxes = new Mailboxes();
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

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.IsRuning = false;

            instance = null;
        }
    }
}