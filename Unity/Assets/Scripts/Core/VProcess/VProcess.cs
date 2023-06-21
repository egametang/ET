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
        }

#region AddComponent

        public new K AddComponent<K>(bool isFromPool = false) where K : SingletonEntity<K>, IAwake, new()
        {
            return base.AddComponent<K>(isFromPool);
        }

        public new K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : SingletonEntity<K>, IAwake<P1>, new()
        {
            return base.AddComponent<K, P1>(p1, isFromPool);
        }

        public new K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : SingletonEntity<K>, IAwake<P1, P2>, new()
        {
            return base.AddComponent<K, P1, P2>(p1, p2, isFromPool);
        }

        public new K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : SingletonEntity<K>, IAwake<P1, P2, P3>, new()
        {
            return base.AddComponent<K, P1, P2, P3>(p1, p2, p3, isFromPool);
        }
        
        public new K AddComponentWithId<K>(long id, bool isFromPool = false) where K : SingletonEntity<K>, IAwake, new()
        {
            return base.AddComponentWithId<K>(id, isFromPool);
        }

        public new K AddComponentWithId<K, P1>(long id, P1 p1, bool isFromPool = false) where K : SingletonEntity<K>, IAwake<P1>, new()
        {
            return base.AddComponentWithId<K, P1>(id, p1, isFromPool);
        }

        public new K AddComponentWithId<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false) where K : SingletonEntity<K>, IAwake<P1, P2>, new()
        {
            return base.AddComponentWithId<K, P1, P2>(id, p1, p2, isFromPool);
        }

        public new K AddComponentWithId<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : SingletonEntity<K>, IAwake<P1, P2, P3>, new()
        {
            return base.AddComponentWithId<K, P1, P2, P3>(id, p1, p2, p3, isFromPool);
        }
        
#endregion
    }
}