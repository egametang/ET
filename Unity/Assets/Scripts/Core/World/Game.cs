using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public class Game
    {
        [StaticField]
        public static Game Instance = new Game();
        
        private Game()
        {
        }
        
        private readonly Stack<ISingleton> singletons = new();

        private readonly Queue<ISingleton> updates = new();

        private readonly Queue<ISingleton> lateUpdates = new();

        private readonly Queue<ISingleton> loads = new();

        private readonly ConcurrentQueue<Process> loops = new();

        private readonly ConcurrentDictionary<int, Process> processes = new();

        private int idGenerate;

        public Process Create(bool loop = true)
        {
            Process process = new(++this.idGenerate);
            this.processes.TryAdd(process.Id, process);
            if (loop)
            {
                this.loops.Enqueue(process);
            }
            return process;
        }
        
        public void Remove(int id)
        {
            if (this.processes.Remove(id, out Process thread))
            {
                thread.Dispose();    
            }
        }
        
        
        // 简单线程调度，每次Loop会把所有Game Loop一遍
        public void Loop()
        {
            int count = this.loops.Count;

            using Barrier barrier = new Barrier(1);
            
            while (count-- > 0)
            {
                this.loops.TryDequeue(out Process thread);
                if (thread == null)
                {
                    continue;
                }
                barrier.AddParticipant();
                thread.Barrier = barrier;
                if (thread.Id == 0)
                {
                    continue;
                }
                this.loops.Enqueue(thread);
                ThreadPool.QueueUserWorkItem(thread.Loop);
            }

            barrier.SignalAndWait();
        }

        public void Send(int threadId, MessageObject messageObject)
        {
            if (this.processes.TryGetValue(threadId, out Process thread))
            {
                return;
            }
            thread.AddMessage(messageObject);
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
        
        public void Update()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                ISingleton singleton = updates.Dequeue();

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
        
        public void LateUpdate()
        {
            int count = lateUpdates.Count;
            while (count-- > 0)
            {
                ISingleton singleton = lateUpdates.Dequeue();
                
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

        public void Load()
        {
            int count = loads.Count;
            while (count-- > 0)
            {
                ISingleton singleton = loads.Dequeue();
                
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