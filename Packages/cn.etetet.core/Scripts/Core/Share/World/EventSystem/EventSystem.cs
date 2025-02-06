using System;
using System.Collections.Generic;

namespace ET
{
    [CodeProcess]
    public class EventSystem: Singleton<EventSystem>, ISingletonAwake
    {
        private class EventInfo
        {
            public IEvent IEvent { get; }
            
            public int SceneType {get; }

            public EventInfo(IEvent iEvent, int sceneType)
            {
                this.IEvent = iEvent;
                this.SceneType = sceneType;
            }
        }
        
        private readonly Dictionary<Type, List<EventInfo>> allEvents = new();
        
        private readonly Dictionary<Type, Dictionary<long, object>> allInvokers = new(); 
        
        public void Awake()
        {
            CodeTypes codeTypes = CodeTypes.Instance;
            foreach (Type type in codeTypes.GetTypes(typeof (EventAttribute)))
            {
                IEvent obj = Activator.CreateInstance(type) as IEvent;
                if (obj == null)
                {
                    throw new Exception($"type not is AEvent: {type.Name}");
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
                foreach (object attr in attrs)
                {
                    EventAttribute eventAttribute = attr as EventAttribute;

                    Type eventType = obj.Type;

                    EventInfo eventInfo = new(obj, eventAttribute.SceneType);

                    if (!this.allEvents.ContainsKey(eventType))
                    {
                        this.allEvents.Add(eventType, new List<EventInfo>());
                    }
                    this.allEvents[eventType].Add(eventInfo);
                }
            }

            foreach (Type type in codeTypes.GetTypes(typeof (InvokeAttribute)))
            {
                object obj = Activator.CreateInstance(type);
                IInvoke iInvoke = obj as IInvoke;
                if (iInvoke == null)
                {
                    throw new Exception($"type not is callback: {type.Name}");
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(InvokeAttribute), false);
                foreach (object attr in attrs)
                {
                    if (!this.allInvokers.TryGetValue(iInvoke.Type, out var dict))
                    {
                        dict = new Dictionary<long, object>();
                        this.allInvokers.Add(iInvoke.Type, dict);
                    }
                    
                    InvokeAttribute invokeAttribute = attr as InvokeAttribute;
                    
                    try
                    {
                        dict.Add(invokeAttribute.Type, obj);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"action type duplicate: {iInvoke.Type.Name} {invokeAttribute.Type}", e);
                    }
                }
            }
        }
        
        public async ETTask PublishAsync<S, T>(S scene, T a) where S: class, IScene where T : struct
        {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using ListComponent<ETTask> list = ListComponent<ETTask>.Create();
            
            foreach (EventInfo eventInfo in iEvents)
            {
                if (!SceneTypeSingleton.IsSame(scene.SceneType, eventInfo.SceneType))
                {
                    continue;
                }
                    
                if (!(eventInfo.IEvent is AEvent<S, T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().FullName}");
                    continue;
                }

                list.Add(aEvent.Handle(scene, a));
            }

            try
            {
                await ETTaskHelper.WaitAll(list);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Publish<S, T>(S scene, T a) where S: class, IScene where T : struct
        {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof (T), out iEvents))
            {
                return;
            }

            int sceneType = scene.SceneType;
            foreach (EventInfo eventInfo in iEvents)
            {
                if (!SceneTypeSingleton.IsSame(sceneType, eventInfo.SceneType))
                {
                    continue;
                }

                
                if (!(eventInfo.IEvent is AEvent<S, T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().FullName}");
                    continue;
                }
                
                aEvent.Handle(scene, a).NoContext();
            }
        }
        
        // Invoke跟Publish的区别(特别注意)
        // Invoke类似函数，必须有被调用方，否则异常，调用者跟被调用者属于同一模块，比如MoveComponent中的Timer计时器，调用跟被调用的代码均属于移动模块
        // 既然Invoke跟函数一样，那么为什么不使用函数呢? 因为有时候不方便直接调用，比如Config加载，在客户端跟服务端加载方式不一样。比如TimerComponent需要根据Id分发
        // 注意，不要把Invoke当函数使用，这样会造成代码可读性降低，能用函数不要用Invoke
        // publish是事件，抛出去可以没人订阅，调用者跟被调用者属于两个模块，比如任务系统需要知道道具使用的信息，则订阅道具使用事件
        public void Invoke<A>(long type, A args) where A: struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error1: {type} {typeof(A).FullName}");
            }
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error2: {type} {typeof(A).FullName}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error3, not AInvokeHandler: {type} {typeof(A).FullName}");
            }
            
            aInvokeHandler.Handle(args);
        }
        
        public T Invoke<A, T>(long type, A args) where A: struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error4: {type} {typeof(A).FullName}");
            }
            
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error5: {type} {typeof(A).FullName}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A, T>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error6, not AInvokeHandler: {type} {typeof(A).FullName} {typeof(T).FullName} ");
            }
            
            return aInvokeHandler.Handle(args);
        }
        
        public void Invoke<A>(A args) where A: struct
        {
            Invoke(0, args);
        }
        
        public T Invoke<A, T>(A args) where A: struct
        {
            return Invoke<A, T>(0, args);
        }
    }
}
