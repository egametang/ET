using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class Process: IDisposable
    {
        public int Id { get; private set; }
        
        public Barrier Barrier { get; set; }

        public Process(int id)
        {
            this.Id = id;

            this.loop = (_) =>
            {
                this.Init();
                this.Update();
                this.LateUpdate();
                this.FrameFinishUpdate();
                this.Barrier?.RemoveParticipant();
            };
        }

        private readonly ConcurrentQueue<MessageObject> Messages = new();
        
        private readonly Stack<IProcessSingleton> singletons = new();

        private readonly Queue<IProcessSingleton> updates = new();

        private readonly Queue<IProcessSingleton> lateUpdates = new();
        
        private readonly Queue<IProcessSingleton> loads = new();

        private readonly Queue<ETTask> frameFinishTask = new();

        private readonly Dictionary<int, ETTask<IResponse>> requestCallbacks = new();

        private readonly WaitCallback loop;

        private int rpcId;

        public void AddMessage(MessageObject messageObject)
        {
            this.Messages.Enqueue(messageObject);
        }

        public void HandleMessage(MessageObject messageObject)
        {
            switch (messageObject)
            {
                case IResponse iResponse:
                {
                    if (this.requestCallbacks.TryGetValue(iResponse.RpcId, out ETTask<IResponse> task))
                    {
                        task.SetResult(iResponse);
                    }
                    break;
                }
                case IRequest iRequest:
                {
                    break;
                }
                default: // IMessage:
                {
                    break;
                }
            }
        }

        private void Init()
        {
            foreach (IProcessSingleton singleton in this.singletons)
            {
                singleton.Register();
            }
        }
        
        public T AddSingleton<T>() where T: ProcessSingleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(IProcessSingleton singleton)
        {
            singleton.Process = this;
            
            singleton.Register();
            
            singletons.Push(singleton);
            
            if (singleton is IProcessSingletonAwake awake)
            {
                awake.Awake();
            }
            
            if (singleton is IProcessSingletonUpdate)
            {
                updates.Enqueue(singleton);
            }
            
            if (singleton is IProcessSingletonLateUpdate)
            {
                lateUpdates.Enqueue(singleton);
            }

            if (singleton is IProcessSingletonLoad)
            {
                loads.Enqueue(singleton);
            }
        }

        public async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }
        
        public void Update()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                IProcessSingleton singleton = updates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not IProcessSingletonUpdate update)
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
                IProcessSingleton singleton = lateUpdates.Dequeue();
                
                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not IProcessSingletonLateUpdate lateUpdate)
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

        public WaitCallback Loop
        {
            get
            {
                return this.loop;
            }
        }
        
        public void Load()
        {
            int count = loads.Count;
            while (count-- > 0)
            {
                IProcessSingleton singleton = loads.Dequeue();
                
                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not IProcessSingletonLoad load)
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

        public void FrameFinishUpdate()
        {
            while (frameFinishTask.Count > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }

        public void Send(int threadId, MessageObject messageObject)
        {
            Game.Instance.Send(threadId, messageObject);
        }
        
        public async ETTask<IResponse> Call(int threadId, IRequest request)
        {
            ETTask<IResponse> task = ETTask<IResponse>.Create(true);
            request.RpcId = ++this.rpcId;
            this.requestCallbacks.Add(request.RpcId, task);
            Game.Instance.Send(threadId, request as MessageObject);
            return await task;
        }

        public void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }
            
            this.Id = 0;
            
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                IProcessSingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
        }
    }
}