using System;
using System.Collections.Generic;

namespace ET
{
    public class VProcess: Entity, IScene, IEntitySystem, IIdGenerater
    {
        [ThreadStatic]
        [StaticField]
        public static VProcess Instance;
        
        public IScene Root { get; set; }
        public SceneType SceneType { get; set; }
        
        public int Process { get; private set; }
        
        public EntitySystem EntitySystem { get; }
        public TimeInfo TimeInfo { get; }
        public IdGenerater IdGenerater { get; }

        public bool IsRuning;
        
        // actor
        private readonly Dictionary<long, Entity> actors = new();

        public VProcess(int process, int id)
        {
            this.Id = id;
            this.Process = process;
            this.Root = this;
            this.EntitySystem = new EntitySystem();
            this.TimeInfo = new TimeInfo();
            this.IdGenerater = new IdGenerater(process, this.TimeInfo);
        }

        public void Update()
        {
            this.TimeInfo.Update();
            
            this.EntitySystem.Update();
        }
        
        public void LateUpdate()
        {
            this.EntitySystem.LateUpdate();

            FrameFinishUpdate();
        }

        public async ETTask WaitFrameFinish()
        {
            
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
        }
    }
}