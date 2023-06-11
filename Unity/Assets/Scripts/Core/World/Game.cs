using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public class Game
    {
        [StaticField]
        public static Game Instance = new();
        
        private Game()
        {
        }
        
        private readonly ConcurrentStack<ISingleton> singletons = new();

        private readonly ConcurrentQueue<ISingleton> updates = new();

        private readonly ConcurrentQueue<ISingleton> lateUpdates = new();

        private readonly ConcurrentQueue<ISingleton> loads = new();

        #region 线程安全

        private bool needLoad;
        
        private readonly ConcurrentQueue<Process> loops = new();

        private readonly ConcurrentDictionary<int, Process> processes = new();
        
        private readonly Queue<ETTask> frameFinishTask = new();

        private int idGenerator;

        public Process Create(bool loop = true)
        {
            int id = Interlocked.Increment(ref this.idGenerator);
            Process process = new(id);
            this.processes.TryAdd(process.Id, process);
            if (loop)
            {
                this.loops.Enqueue(process);
            }
            return process;
        }
        
        public void Remove(int id)
        {
            if (this.processes.Remove(id, out Process process))
            {
                process.Dispose();    
            }
        }
        
        public async ETTask WaitGameFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }
        
        private void FrameFinishUpdateInner()
        {
            while (frameFinishTask.Count > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }

        public void Load()
        {
            this.needLoad = true;
        }
        
        // 简单线程调度，每次Loop会把所有Process Loop一遍
        public void Loop()
        {
            int count = this.loops.Count;

            using Barrier barrier = new(1);
            
            while (count-- > 0)
            {
                this.loops.TryDequeue(out Process process);
                if (process == null)
                {
                    continue;
                }
                barrier.AddParticipant();
                process.Barrier = barrier;
                if (process.Id == 0)
                {
                    continue;
                }
                this.loops.Enqueue(process);
                ThreadPool.QueueUserWorkItem(process.Loop);
            }

            barrier.SignalAndWait();
            
            // 此时没有线程竞争，进行 Load Update LateUpdate等操作
            if (this.needLoad)
            {
                this.needLoad = false;
                this.LoadInner();
            }
            this.UpdateInner();
            this.LateUpdateInner();
            this.FrameFinishUpdateInner();
        }

        #endregion


        public void Send(int processId, MessageObject messageObject)
        {
            if (this.processes.TryGetValue(processId, out Process process))
            {
                return;
            }
            process.AddMessage(messageObject);
        }
        
        // 为了保证线程安全，只允许在Start之前AddSingleton，主要用于线程共用的一些东西
        public T AddSingleton<T>() where T: Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(ISingleton singleton)
        {
            singleton.Register();
            
            singletons.Push(singleton);
            
            if (singleton is ISingletonAwake awake)
            {
                awake.Awake();
            }
            
            if (singleton is ISingletonUpdate)
            {
                updates.Enqueue(singleton);
            }
            
            if (singleton is ISingletonLateUpdate)
            {
                lateUpdates.Enqueue(singleton);
            }

            if (singleton is ISingletonLoad)
            {
                loads.Enqueue(singleton);
            }
        }

        private void UpdateInner()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                if (!updates.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonUpdate update)
                {
                    continue;
                }
                
                updates.Enqueue(singleton);
                try
                {
                    update.Update();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private void LateUpdateInner()
        {
            int count = lateUpdates.Count;
            while (count-- > 0)
            {
                if (!lateUpdates.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonLateUpdate lateUpdate)
                {
                    continue;
                }
                
                lateUpdates.Enqueue(singleton);
                try
                {
                    lateUpdate.LateUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private void LoadInner()
        {
            int count = loads.Count;
            while (count-- > 0)
            {
                if (!this.loads.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }
                
                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonLoad load)
                {
                    continue;
                }
                
                loads.Enqueue(singleton);
                try
                {
                    load.Load();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}