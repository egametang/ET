using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ET
{
    using OneTypeSystems = UnOrderMultiMap<Type, object>;

    public class EventSystem: Singleton<EventSystem>
    {
        private class TypeSystems
        {
            private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

            public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                if (systems != null)
                {
                    return systems;
                }

                systems = new OneTypeSystems();
                this.typeSystemsMap.Add(type, systems);
                return systems;
            }

            public OneTypeSystems GetOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                return systems;
            }

            public List<object> GetSystems(Type type, Type systemType)
            {
                OneTypeSystems oneTypeSystems = null;
                if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
                {
                    return null;
                }

                if (!oneTypeSystems.TryGetValue(systemType, out List<object> systems))
                {
                    return null;
                }

                return systems;
            }
        }

        private class EventInfo
        {
            public IEvent IEvent { get; }
            
            public SceneType SceneType {get; }

            public EventInfo(IEvent iEvent, SceneType sceneType)
            {
                this.IEvent = iEvent;
                this.SceneType = sceneType;
            }
        }
        
        private class TwoQueue
        {
            public Queue<long> Queue1 = new();
            public Queue<long> Queue2 = new();

            public void Swap()
            {
                (this.Queue1, this.Queue2) = (this.Queue2, this.Queue1);
            }
        }

        private readonly Dictionary<long, Entity> allEntities = new();

        private readonly Dictionary<string, Type> allTypes = new();

        private readonly UnOrderMultiMapSet<Type, Type> types = new();

        private readonly Dictionary<Type, List<EventInfo>> allEvents = new();
        
        private Dictionary<Type, Dictionary<int, object>> allCallbacks = new Dictionary<Type, Dictionary<int, object>>(); 

        private TypeSystems typeSystems = new();

        private readonly TwoQueue[] twoQueues = new TwoQueue[(int)TwoQueueEnum.Max];

        private enum TwoQueueEnum
        {
            Update = 0,
            LateUpdate = 1,
            Load = 2,
            Max = 3,
        }

        public EventSystem()
        {
            for (int i = 0; i < this.twoQueues.Length; i++)
            {
                this.twoQueues[i] = new TwoQueue();
            }
        }

        private static List<Type> GetBaseAttributes(Dictionary<string, Type> addTypes)
        {
            List<Type> attributeTypes = new List<Type>();
            foreach (Type type in addTypes.Values)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (type.IsSubclassOf(typeof (BaseAttribute)))
                {
                    attributeTypes.Add(type);
                }
            }

            return attributeTypes;
        }

        public void Add(Dictionary<string, Type> addTypes)
        {
            this.allTypes.Clear();
            foreach (Type addType in addTypes.Values)
            {
                this.allTypes[addType.FullName] = addType;
            }

            this.types.Clear();
            List<Type> baseAttributeTypes = GetBaseAttributes(addTypes);
            foreach (Type baseAttributeType in baseAttributeTypes)
            {
                foreach (Type type in addTypes.Values)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    object[] objects = type.GetCustomAttributes(baseAttributeType, true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }

                    this.types.Add(baseAttributeType, type);
                }
            }

            this.typeSystems = new TypeSystems();

            foreach (Type type in this.GetTypes(typeof (ObjectSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    OneTypeSystems oneTypeSystems = this.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Add(iSystemType.SystemType(), obj);
                }
            }

            this.allEvents.Clear();
            foreach (Type type in types[typeof (EventAttribute)])
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

                    Type eventType = obj.GetEventType();

                    EventInfo eventInfo = new(obj, eventAttribute.SceneType);

                    if (!this.allEvents.ContainsKey(eventType))
                    {
                        this.allEvents.Add(eventType, new List<EventInfo>());
                    }
                    this.allEvents[eventType].Add(eventInfo);
                }
            }

            this.allCallbacks = new Dictionary<Type, Dictionary<int, object>>();
            foreach (Type type in types[typeof (CallbackAttribute)])
            {
                object obj = Activator.CreateInstance(type);
                ICallbackType iCallbackType = obj as ICallbackType;
                if (iCallbackType == null)
                {
                    throw new Exception($"type not is callback: {type.Name}");
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(CallbackAttribute), false);
                foreach (object attr in attrs)
                {
                    if (!this.allCallbacks.TryGetValue(iCallbackType.Type, out var dict))
                    {
                        dict = new Dictionary<int, object>();
                        this.allCallbacks.Add(iCallbackType.Type, dict);
                    }
                    
                    CallbackAttribute callbackAttribute = attr as CallbackAttribute;
                    
                    try
                    {
                        dict.Add(callbackAttribute.Id, obj);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"action type duplicate: {iCallbackType.Type.Name} {callbackAttribute.Id}", e);
                    }
                    
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return this.allTypes[typeName];
        }

        public void RegisterSystem(Entity component, bool isRegister = true)
        {
            if (!isRegister)
            {
                this.Remove(component.InstanceId);
                return;
            }

            this.allEntities.Add(component.InstanceId, component);

            Type type = component.GetType();

            OneTypeSystems oneTypeSystems = this.typeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }
            if (component is ILoad)
            {
                if (oneTypeSystems.ContainsKey(typeof (ILoadSystem)))
                {
                    this.twoQueues[(int)TwoQueueEnum.Load].Queue1.Enqueue(component.InstanceId);
                }
            }

            if (component is IUpdate)
            {
                if (oneTypeSystems.ContainsKey(typeof (IUpdateSystem)))
                {
                    this.twoQueues[(int)TwoQueueEnum.Update].Queue1.Enqueue(component.InstanceId);
                }
            }

            if (component is ILateUpdate)
            {
                if (oneTypeSystems.ContainsKey(typeof (ILateUpdateSystem)))
                {
                    this.twoQueues[(int)TwoQueueEnum.LateUpdate].Queue1.Enqueue(component.InstanceId);
                }
            }
        }

        public void Remove(long instanceId)
        {
            this.allEntities.Remove(instanceId);
        }

        public Entity Get(long instanceId)
        {
            Entity component = null;
            this.allEntities.TryGetValue(instanceId, out component);
            return component;
        }

        public bool IsRegister(long instanceId)
        {
            return this.allEntities.ContainsKey(instanceId);
        }

        public void Deserialize(Entity component)
        {
            List<object> iDeserializeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDeserializeSystem));
            if (iDeserializeSystems == null)
            {
                return;
            }

            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
            {
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // GetComponentSystem
        public void GetComponent(Entity entity, Entity component)
        {
            List<object> iGetSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IGetComponentSystem));
            if (iGetSystem == null)
            {
                return;
            }

            foreach (IGetComponentSystem getSystem in iGetSystem)
            {
                if (getSystem == null)
                {
                    continue;
                }

                try
                {
                    getSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // AddComponentSystem
        public void AddComponent(Entity entity, Entity component)
        {
            List<object> iAddSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IAddComponentSystem));
            if (iAddSystem == null)
            {
                return;
            }

            foreach (IAddComponentSystem addComponentSystem in iAddSystem)
            {
                if (addComponentSystem == null)
                {
                    continue;
                }

                try
                {
                    addComponentSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake(Entity component)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1>(Entity component, P1 p1)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Load()
        {
            TwoQueue twoQueue = this.twoQueues[(int)TwoQueueEnum.Load];
            while (twoQueue.Queue1.Count > 0)
            {
                long instanceId = twoQueue.Queue1.Dequeue();
                Entity component;
                if (!this.allEntities.TryGetValue(instanceId, out component))
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iLoadSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILoadSystem));
                if (iLoadSystems == null)
                {
                    continue;
                }

                twoQueue.Queue2.Enqueue(instanceId);

                foreach (ILoadSystem iLoadSystem in iLoadSystems)
                {
                    try
                    {
                        iLoadSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            twoQueue.Swap();
        }

        public void Destroy(Entity component)
        {
            List<object> iDestroySystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Update()
        {
            TwoQueue twoQueue = this.twoQueues[(int)TwoQueueEnum.Update];
            while (twoQueue.Queue1.Count > 0)
            {
                long instanceId = twoQueue.Queue1.Dequeue();
                Entity component;
                if (!this.allEntities.TryGetValue(instanceId, out component))
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
                if (iUpdateSystems == null)
                {
                    continue;
                }

                twoQueue.Queue2.Enqueue(instanceId);

                foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
                {
                    try
                    {
                        iUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            twoQueue.Swap();
        }

        public void LateUpdate()
        {
            TwoQueue twoQueue = this.twoQueues[(int)TwoQueueEnum.LateUpdate];
            while (twoQueue.Queue1.Count > 0)
            {
                long instanceId = twoQueue.Queue1.Dequeue();
                Entity component;
                if (!this.allEntities.TryGetValue(instanceId, out component))
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iLateUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                twoQueue.Queue2.Enqueue(instanceId);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            twoQueue.Swap();
        }

        public async ETTask PublishAsync<T>(Scene scene, T a) where T : struct
        {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using ListComponent<ETTask> list = ListComponent<ETTask>.Create();
            
            foreach (EventInfo eventInfo in iEvents)
            {
                if (scene.SceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None)
                {
                    continue;
                }
                    
                if (!(eventInfo.IEvent is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
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

        public void Publish<T>(Scene scene, T a)where T : struct
        {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof (T), out iEvents))
            {
                return;
            }

            SceneType sceneType = scene.SceneType;
            foreach (EventInfo eventInfo in iEvents)
            {
                if (sceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None)
                {
                    continue;
                }

                
                if (!(eventInfo.IEvent is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }
                
                aEvent.Handle(scene, a).Coroutine();
            }
        }
        
        public void Callback<A>(A args) where A: struct, ICallback
        {
            if (!this.allCallbacks.TryGetValue(typeof(A), out var callbackHandlers))
            {
                throw new Exception($"Callback error: {typeof(A).Name}");
            }
            if (!callbackHandlers.TryGetValue(args.Id, out var callbackHandler))
            {
                throw new Exception($"Callback error: {typeof(A).Name} {args.Id}");
            }

            var aCallbackHandler = callbackHandler as ACallbackHandler<A>;
            if (aCallbackHandler == null)
            {
                throw new Exception($"Callback error, not ACallbackHandler: {typeof(A).Name} {args.Id}");
            }
            
            aCallbackHandler.Handle(args);
        }
        
        public T Callback<A, T>(A args) where A: struct, ICallback
        {
            if (!this.allCallbacks.TryGetValue(typeof(A), out var callbackHandlers))
            {
                throw new Exception($"ResultCallback error: {typeof(A).Name}");
            }
            if (!callbackHandlers.TryGetValue(args.Id, out var callbackHandler))
            {
                throw new Exception($"ResultCallback error: {typeof(A).Name} {args.Id}");
            }

            var aCallbackHandler = callbackHandler as ACallbackHandler<A, T>;
            if (aCallbackHandler == null)
            {
                throw new Exception($"ResultCallback error, not AResultCallbackHandler: {typeof(T).Name} {args.Id}");
            }
            
            return aCallbackHandler.Handle(args);
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            HashSet<Type> noParent = new HashSet<Type>();
            Dictionary<Type, int> typeCount = new Dictionary<Type, int>();

            HashSet<Type> noDomain = new HashSet<Type>();

            foreach (var kv in this.allEntities)
            {
                Type type = kv.Value.GetType();
                if (kv.Value.Parent == null)
                {
                    noParent.Add(type);
                }

                if (kv.Value.Domain == null)
                {
                    noDomain.Add(type);
                }

                if (typeCount.ContainsKey(type))
                {
                    typeCount[type]++;
                }
                else
                {
                    typeCount[type] = 1;
                }
            }

            sb.AppendLine("not set parent type: ");
            foreach (Type type in noParent)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            sb.AppendLine("not set domain type: ");
            foreach (Type type in noDomain)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            IOrderedEnumerable<KeyValuePair<Type, int>> orderByDescending = typeCount.OrderByDescending(s => s.Value);

            sb.AppendLine("Entity Count: ");
            foreach (var kv in orderByDescending)
            {
                if (kv.Value == 1)
                {
                    continue;
                }

                sb.AppendLine($"\t{kv.Key.Name}: {kv.Value}");
            }

            return sb.ToString();
        }
    }
}
