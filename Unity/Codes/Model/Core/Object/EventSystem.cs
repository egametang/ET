using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ET
{
    public sealed class EventSystem: IDisposable
    {
        private class TypeSystems
        {
            //key为 ISystemType接口类型继承者的<T>泛型类型  value=
            private readonly Dictionary<Type, UnOrderMultiMap<Type, object>> typeSystemsMap = new Dictionary<Type, UnOrderMultiMap<Type, object>>();

            public UnOrderMultiMap<Type, object> GetOrCreateOneTypeSystems(Type type)
            {
                UnOrderMultiMap<Type, object> systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                if (systems != null)
                {
                    return systems;
                }

                systems = new UnOrderMultiMap<Type, object>();
                this.typeSystemsMap.Add(type, systems);
                return systems;
            }

            public UnOrderMultiMap<Type, object> GetOneTypeSystems(Type type)
            {
                UnOrderMultiMap<Type, object> systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                return systems;
            }

            public List<object> GetSystems(Type type, Type systemType)
            {
                UnOrderMultiMap<Type, object> oneTypeSystems = null;
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

        private static EventSystem instance;

        public static EventSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventSystem();
                }

                return instance;
            }
        }

        private readonly Dictionary<long, Entity> allEntities = new Dictionary<long, Entity>();

        private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        private readonly Dictionary<string, Type> allTypes = new Dictionary<string, Type>(); //已经解析所有的运行类型
 
        private readonly UnOrderMultiMapSet<Type, Type> types = new UnOrderMultiMapSet<Type, Type>(); //key=baseAttributeType  value=HashSet<Type>指定的哈希集合

        private readonly Dictionary<Type, List<object>> allEvents = new Dictionary<Type, List<object>>(); //key=AEvent<A> A的类型  value=具体需要处理的事件类集合

        private TypeSystems typeSystems = new TypeSystems();

        private Queue<long> updates = new Queue<long>();
        private Queue<long> updates2 = new Queue<long>();

        private Queue<long> loaders = new Queue<long>();
        private Queue<long> loaders2 = new Queue<long>();

        private Queue<long> lateUpdates = new Queue<long>();
        private Queue<long> lateUpdates2 = new Queue<long>();

        private EventSystem()
        {
        }

        private static List<Type> GetBaseAttributes(Type[] addTypes)
        {
            List<Type> attributeTypes = new List<Type>();
            foreach (Type type in addTypes)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                //判断当前类型 是否标记有 BaseAttribute属性的类型
                //如果有代表 我们就需要这些类型
                if (type.IsSubclassOf(typeof (BaseAttribute)))
                {
                    attributeTypes.Add(type);
                }
            }

            return attributeTypes;
        }

        //总结一下Add方法干了什么事情
        //解析了自定义属性 BaseAttribute 得到了types
        //解析了ObjectSystemAttribute 也就是我们的所有的生命周期流程管理类
        //解析了EventAttribute 得到了我们需要分发的事件管理类
        public void Add(Type[] addTypes)
        {
            this.allTypes.Clear();
            foreach (Type addType in addTypes)
            {
                this.allTypes[addType.FullName] = addType;
            }

            this.types.Clear();
            List<Type> baseAttributeTypes = GetBaseAttributes(addTypes);
            foreach (Type baseAttributeType in baseAttributeTypes)
            {
                foreach (Type type in addTypes)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }
 
                    // type 是否为 baseAttributeType 派生下来的类型 
                    object[] objects = type.GetCustomAttributes(baseAttributeType, true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }

                    //如果type是baseAttributeType 派生下来的类型,添加进入属性的哈希集合
                    this.types.Add(baseAttributeType, type);
                }
            }

            this.typeSystems = new TypeSystems();

            foreach (Type type in this.GetTypes(typeof (ObjectSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    //通过iSystemType 得到一个无序的集合列表
                    UnOrderMultiMap<Type, object> oneTypeSystems = this.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Add(iSystemType.SystemType(), obj);
                }
            }

            this.allEvents.Clear();
            foreach (Type type in types[typeof (EventAttribute)])
            {
                IEvent obj = Activator.CreateInstance(type) as IEvent;
                if (obj == null)
                {
                    throw new Exception($"type not is AEvent: {obj.GetType().Name}");
                }

                //GetEventType方法 其实是获取到泛型T 的指定类型
                Type eventType = obj.GetEventType();
                if (!this.allEvents.ContainsKey(eventType))
                {
                    this.allEvents.Add(eventType, new List<object>());
                }

                this.allEvents[eventType].Add(obj);
            }
        }

        public void Add(Assembly assembly)
        {
            this.assemblies[$"{assembly.GetName().Name}.dll"] = assembly;

            List<Type> addTypes = new List<Type>();

            foreach (Assembly ass in this.assemblies.Values)
            {
                addTypes.AddRange(ass.GetTypes());
            }

            this.Add(addTypes.ToArray());
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

            UnOrderMultiMap<Type, object> oneTypeSystems = this.typeSystems.GetOneTypeSystems(type);;
            if (component is ILoad)
            {
                if (oneTypeSystems.ContainsKey(typeof (ILoadSystem)))
                {
                    this.loaders.Enqueue(component.InstanceId);
                }
            }

            if (component is IUpdate)
            {
                if (oneTypeSystems.ContainsKey(typeof (IUpdateSystem)))
                {
                    this.updates.Enqueue(component.InstanceId);
                }
            }

            if (component is ILateUpdate)
            {
                if (oneTypeSystems.ContainsKey(typeof (ILateUpdateSystem)))
                {
                    this.lateUpdates.Enqueue(component.InstanceId);
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
            while (this.loaders.Count > 0)
            {
                long instanceId = this.loaders.Dequeue();
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

                this.loaders2.Enqueue(instanceId);

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

            ObjectHelper.Swap(ref this.loaders, ref this.loaders2);
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
            while (this.updates.Count > 0)
            {
                long instanceId = this.updates.Dequeue();
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

                this.updates2.Enqueue(instanceId);

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

            ObjectHelper.Swap(ref this.updates, ref this.updates2);
        }

        public void LateUpdate()
        {
            while (this.lateUpdates.Count > 0)
            {
                long instanceId = this.lateUpdates.Dequeue();
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

                this.lateUpdates2.Enqueue(instanceId);

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

            ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
        }

        public async ETTask PublishAsync<T>(T a) where T : struct
        {
            List<object> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using (ListComponent<ETTask> list = ListComponent<ETTask>.Create())
            {
                foreach (object obj in iEvents)
                {
                    if (!(obj is AEvent<T> aEvent))
                    {
                        Log.Error($"event error: {obj.GetType().Name}");
                        continue;
                    }

                    list.Add(aEvent.Handle(a));
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
        }

        public void Publish<T>(T a) where T : struct
        {
            List<object> iEvents;
            if (!this.allEvents.TryGetValue(typeof (T), out iEvents))
            {
                return;
            }

            foreach (object obj in iEvents)
            {
                if (!(obj is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {obj.GetType().Name}");
                    continue;
                }
                aEvent.Handle(a).Coroutine();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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

        public void Dispose()
        {
            instance = null;
        }
    }
}