//#undef SINGLE_THREAD

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;



namespace ET
{
    public enum NetworkProtocol
    {
        TCP,
        KCP,
        Websocket,
    }

    public enum NetOp: byte
    {
        AddService = 1,
        RemoveService = 2,
        OnAccept = 3,
        OnRead = 4,
        OnError = 5,
        CreateChannel = 6,
        RemoveChannel = 7,
        SendMessage = 9,
        GetChannelConn = 10,
        ChangeAddress = 11,
    }

    public struct NetOperator
    {
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public long ActorId;
        public object Object; // 参数
    }

    public class NetServices: VProcessSingleton<NetServices>, ISingletonUpdate
    {
#if !SINGLE_THREAD
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>();
        private readonly ConcurrentQueue<NetOperator> mainThreadOperators = new ConcurrentQueue<NetOperator>();
#endif

        public NetServices()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                this.typeOpcode.Add(type, messageAttribute.Opcode);
            }

#if !SINGLE_THREAD
            // 网络线程
            this.thread = new System.Threading.Thread(this.NetThreadUpdate);
            this.thread.Start();
#endif
        }

        public void Destroy()
        {
#if !SINGLE_THREAD
            this.isStop = true;
            this.thread.Join(1000);
#endif
        }

        #region 线程安全

        private readonly MessagePool messagePool = new();

        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new();

        public ushort GetOpcode(Type type)
        {
            return this.typeOpcode.GetValueByKey(type);
        }

        public Type GetType(ushort opcode)
        {
            return this.typeOpcode.GetKeyByValue(opcode);
        }

        public MessageObject FetchMessage(Type type)
        {
            return this.messagePool.Fetch(type);
        }
        
        public T FetchMessage<T>() where T: MessageObject
        {
            return this.messagePool.Fetch<T>();
        }

        public void RecycleMessage(MessageObject obj)
        {
            if (obj == null)
            {
                return;
            }
            this.messagePool.Recycle(obj);
        }

        #endregion

        #region 主线程

        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new();
        private readonly Dictionary<int, Action<long, long, object>> readCallback = new();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new();

        private int serviceIdGenerator;

        public async Task<(uint, uint)> GetChannelConn(int serviceId, long channelId)
        {
            TaskCompletionSource<(uint, uint)> tcs = new TaskCompletionSource<(uint, uint)>();
            NetOperator netOperator = new NetOperator() { Op = NetOp.GetChannelConn, ServiceId = serviceId, ChannelId = channelId, Object = tcs };
            ToNetThread(ref netOperator);
            return await tcs.Task;
        }

        private void ToNetThread(ref NetOperator netOperator)
        {
#if !SINGLE_THREAD
            this.netThreadOperators.Enqueue(netOperator);
#else
            NetThreadExecute(ref netOperator);
#endif
        }

        private void ToMainThread(ref NetOperator netOperator)
        {
#if !SINGLE_THREAD
            this.mainThreadOperators.Enqueue(netOperator);
#else
            MainThreadExecute(ref netOperator);
#endif
        }

        public void ChangeAddress(int serviceId, long channelId, IPEndPoint ipEndPoint)
        {
            NetOperator netOperator =
                    new NetOperator() { Op = NetOp.ChangeAddress, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            ToNetThread(ref netOperator);
        }

        public void SendMessage(int serviceId, long channelId, long actorId, MessageObject message)
        {
            NetOperator netOperator = new NetOperator()
            {
                Op = NetOp.SendMessage,
                ServiceId = serviceId,
                ChannelId = channelId,
                ActorId = actorId,
                Object = message
            };
            ToNetThread(ref netOperator);
        }

        public int AddService(AService aService)
        {
            aService.Id = ++this.serviceIdGenerator;
            NetOperator netOperator = new NetOperator() { Op = NetOp.AddService, ServiceId = aService.Id, ChannelId = 0, Object = aService };
            ToNetThread(ref netOperator);
            return aService.Id;
        }

        public void RemoveService(int serviceId)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.RemoveService, ServiceId = serviceId };
            ToNetThread(ref netOperator);
        }

        public void RemoveChannel(int serviceId, long channelId, int error)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.RemoveChannel, ServiceId = serviceId, ChannelId = channelId, ActorId = error };
            ToNetThread(ref netOperator);
        }

        public void CreateChannel(int serviceId, long channelId, IPEndPoint address)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.CreateChannel, ServiceId = serviceId, ChannelId = channelId, Object = address };
            ToNetThread(ref netOperator);
        }

        public void RegisterAcceptCallback(int serviceId, Action<long, IPEndPoint> action)
        {
            this.acceptCallback.Add(serviceId, action);
        }

        public void RegisterReadCallback(int serviceId, Action<long, long, object> action)
        {
            this.readCallback.Add(serviceId, action);
        }

        public void RegisterErrorCallback(int serviceId, Action<long, int> action)
        {
            this.errorCallback.Add(serviceId, action);
        }

        private void MainThreadExecute(ref NetOperator op)
        {
            try
            {
                switch (op.Op)
                {
                    case NetOp.OnAccept:
                    {
                        if (!this.acceptCallback.TryGetValue(op.ServiceId, out var action))
                        {
                            return;
                        }

                        action.Invoke(op.ChannelId, op.Object as IPEndPoint);
                        break;
                    }
                    case NetOp.OnRead:
                    {
                        if (!this.readCallback.TryGetValue(op.ServiceId, out var action))
                        {
                            return;
                        }

                        action.Invoke(op.ChannelId, op.ActorId, op.Object);
                        break;
                    }
                    case NetOp.OnError:
                    {
                        if (!this.errorCallback.TryGetValue(op.ServiceId, out var action))
                        {
                            return;
                        }

                        action.Invoke(op.ChannelId, (int) op.ActorId);
                        break;
                    }
                    default:
                        throw new Exception($"not found net operator: {op.Op}");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void UpdateInMainThread()
        {
#if !SINGLE_THREAD
            while (true)
            {
                if (!this.mainThreadOperators.TryDequeue(out NetOperator op))
                {
                    return;
                }

                MainThreadExecute(ref op);
            }
#endif
        }

        #endregion

        #region 网络线程

        private readonly Dictionary<int, AService> services = new();
        private readonly Queue<int> queue = new();

        private readonly Queue<MemoryBuffer> pool = new();

        public MemoryBuffer FetchMemoryBuffer()
        {
            if (this.pool.Count > 0)
            {
                return this.pool.Dequeue();
            }

            MemoryBuffer memoryBuffer = new(128) { IsFromPool = true };
            return memoryBuffer;
        }

        public void RecycleMemoryBuffer(MemoryBuffer memoryBuffer)
        {
            if (memoryBuffer == null)
            {
                return;
            }
            
            if (!memoryBuffer.IsFromPool)
            {
                return;
            }
            if (memoryBuffer.Capacity > 128) // 太大的不回收，GC
            {
                return;
            }

            if (this.pool.Count > 1000)
            {
                return;
            }

            memoryBuffer.SetLength(0);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            this.pool.Enqueue(memoryBuffer);
        }

        private void Add(AService aService)
        {
            this.services[aService.Id] = aService;
            this.queue.Enqueue(aService.Id);
        }

        public AService Get(int id)
        {
            AService aService;
            this.services.TryGetValue(id, out aService);
            return aService;
        }

        private void Remove(int id)
        {
            if (this.services.Remove(id, out AService service))
            {
                service.Dispose();
            }
        }

#if !SINGLE_THREAD

        private bool isStop;
        private readonly System.Threading.Thread thread;

        // 网络线程Update
        private void NetThreadUpdate()
        {
            while (!this.isStop)
            {
                this.UpdateInNetThread();
                System.Threading.Thread.Sleep(1);
            }

            // 停止的时候再执行一帧，把队列中的消息处理完成
            this.UpdateInNetThread();
        }
#endif

        private void NetThreadExecute(ref NetOperator op)
        {
            try
            {
                switch (op.Op)
                {
                    case NetOp.AddService:
                    {
                        this.Add(op.Object as AService);
                        break;
                    }
                    case NetOp.RemoveService:
                    {
                        this.Remove(op.ServiceId);
                        break;
                    }
                    case NetOp.CreateChannel:
                    {
                        AService service = this.Get(op.ServiceId);
                        if (service != null)
                        {
                            service.Create(op.ChannelId, op.Object as IPEndPoint);
                        }

                        break;
                    }
                    case NetOp.RemoveChannel:
                    {
                        AService service = this.Get(op.ServiceId);
                        if (service != null)
                        {
                            service.Remove(op.ChannelId, (int) op.ActorId);
                        }

                        break;
                    }
                    case NetOp.SendMessage:
                    {
                        AService service = this.Get(op.ServiceId);
                        if (service != null)
                        {
                            service.Send(op.ChannelId, op.ActorId, op.Object as MessageObject);
                        }

                        break;
                    }
                    case NetOp.GetChannelConn:
                    {
                        var tcs = op.Object as TaskCompletionSource<ValueTuple<uint, uint>>;
                        try
                        {
                            AService service = this.Get(op.ServiceId);
                            if (service == null)
                            {
                                break;
                            }

                            tcs.SetResult(service.GetChannelConn(op.ChannelId));
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }

                        break;
                    }
                    case NetOp.ChangeAddress:
                    {
                        AService service = this.Get(op.ServiceId);
                        if (service == null)
                        {
                            break;
                        }

                        service.ChangeAddress(op.ChannelId, op.Object as IPEndPoint);
                        break;
                    }
                    default:
                        throw new Exception($"not found net operator: {op.Op}");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void RunNetThreadOperator()
        {
#if !SINGLE_THREAD
            while (true)
            {
                if (!this.netThreadOperators.TryDequeue(out NetOperator op))
                {
                    return;
                }
                
                NetThreadExecute(ref op);
            }
#endif
        }

        private void UpdateInNetThread()
        {
            int count = this.queue.Count;
            while (count-- > 0)
            {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service))
                {
                    continue;
                }

                this.queue.Enqueue(serviceId);
                service.Update();
            }

            this.RunNetThreadOperator();
        }

        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnAccept, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            ToMainThread(ref netOperator);
        }

        public void OnRead(int serviceId, long channelId, long actorId, object message)
        {
            NetOperator netOperator = new NetOperator()
            {
                Op = NetOp.OnRead,
                ServiceId = serviceId,
                ChannelId = channelId,
                ActorId = actorId,
                Object = message
            };
            ToMainThread(ref netOperator);
        }

        public void OnError(int serviceId, long channelId, int error)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnError, ServiceId = serviceId, ChannelId = channelId, ActorId = error };
            ToMainThread(ref netOperator);
        }

        #endregion

        #region 主线程kcp id生成

        // 这个因为是NetClientComponent中使用，不会与Accept冲突
        public uint CreateConnectChannelId()
        {
            return RandomGenerator.RandUInt32();
        }

        #endregion

        #region 网络线程kcp id生成

        // 防止与内网进程号的ChannelId冲突，所以设置为一个大的随机数
        private uint acceptIdGenerator = uint.MaxValue;

        public uint CreateAcceptChannelId()
        {
            return --this.acceptIdGenerator;
        }

        #endregion

        public void Update()
        {
#if SINGLE_THREAD
            UpdateInNetThread();
#endif
            UpdateInMainThread();
        }
    }
}