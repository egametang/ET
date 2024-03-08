using System;
using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [Flags]
    public enum EntityStatus: byte
    {
        None = 0,
        IsFromPool = 1,
        IsRegister = 1 << 1,
        IsComponent = 1 << 2,
        IsCreated = 1 << 3,
        IsNew = 1 << 4,
    }

    [MemoryPackable(GenerateType.NoGenerate)]
    public abstract partial class Entity: DisposeObject, IPool
    {
#if ENABLE_VIEW && UNITY_EDITOR
        [BsonIgnore]
        [UnityEngine.HideInInspector]
        [MemoryPackIgnore]
        public UnityEngine.GameObject ViewGO;
#endif

        [MemoryPackIgnore]
        [BsonIgnore]
        public long InstanceId { get; protected set; }

        protected Entity()
        {
        }

        [BsonIgnore]
        private EntityStatus status = EntityStatus.None;

        [MemoryPackIgnore]
        [BsonIgnore]
        public bool IsFromPool
        {
            get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this.status &= ~EntityStatus.IsFromPool;
                }
            }
        }

        [BsonIgnore]
        protected bool IsRegister
        {
            get => (this.status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
            set
            {
                if (this.IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    this.status |= EntityStatus.IsRegister;
                }
                else
                {
                    this.status &= ~EntityStatus.IsRegister;
                }

                if (value)
                {
                    this.RegisterSystem();
                }

#if ENABLE_VIEW && UNITY_EDITOR
                if (value)
                {
                    this.ViewGO = new UnityEngine.GameObject(this.ViewName);
                    this.ViewGO.AddComponent<ComponentView>().Component = this;
                    this.ViewGO.transform.SetParent(this.Parent == null? 
                            UnityEngine.GameObject.Find("Global/Scenes").transform : this.Parent.ViewGO.transform);
                }
                else
                {
                    UnityEngine.Object.Destroy(this.ViewGO);
                }
#endif
            }
        }

        protected virtual void RegisterSystem()
        {
            this.iScene.Fiber.EntitySystem.RegisterSystem(this);
        }

        protected virtual string ViewName
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        [BsonIgnore]
        protected bool IsComponent
        {
            get => (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsComponent;
                }
                else
                {
                    this.status &= ~EntityStatus.IsComponent;
                }
            }
        }

        [BsonIgnore]
        protected bool IsCreated
        {
            get => (this.status & EntityStatus.IsCreated) == EntityStatus.IsCreated;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsCreated;
                }
                else
                {
                    this.status &= ~EntityStatus.IsCreated;
                }
            }
        }

        [BsonIgnore]
        protected bool IsNew
        {
            get => (this.status & EntityStatus.IsNew) == EntityStatus.IsNew;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsNew;
                }
                else
                {
                    this.status &= ~EntityStatus.IsNew;
                }
            }
        }

        [MemoryPackIgnore]
        [BsonIgnore]
        public bool IsDisposed => this.InstanceId == 0;
        
        [BsonIgnore]
        private Entity parent;

        // 可以改变parent，但是不能设置为null
        [MemoryPackIgnore]
        [BsonIgnore]
        public Entity Parent
        {
            get => this.parent;
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().FullName}");
                }

                // 严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.IScene == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {this.GetType().FullName} {value.GetType().FullName}");
                }

                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent == value)
                    {
                        Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                        return;
                    }

                    this.parent.RemoveFromChildren(this);
                }

                this.parent = value;
                this.IsComponent = false;
                this.parent.AddToChildren(this);

                if (this is IScene scene)
                {
                    scene.Fiber = this.parent.iScene.Fiber;
                    this.IScene = scene;
                }
                else
                {
                    this.IScene = this.parent.iScene;
                }

#if ENABLE_VIEW && UNITY_EDITOR
                this.ViewGO.GetComponent<ComponentView>().Component = this;
                this.ViewGO.transform.SetParent(this.Parent == null ?
                        UnityEngine.GameObject.Find("Global").transform : this.Parent.ViewGO.transform);
                foreach (var child in this.Children.Values)
                {
                    child.ViewGO.transform.SetParent(this.ViewGO.transform);
                }
                foreach (var comp in this.Components.Values)
                {
                    comp.ViewGO.transform.SetParent(this.ViewGO.transform);
                }
#endif
            }
        }

        // 该方法只能在AddComponent中调用，其他人不允许调用
        [BsonIgnore]
        private Entity ComponentParent
        {
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().FullName}");
                }

                // 严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.IScene == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {this.GetType().FullName} {value.GetType().FullName}");
                }

                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent == value)
                    {
                        Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                        return;
                    }

                    this.parent.RemoveFromComponents(this);
                }

                this.parent = value;
                this.IsComponent = true;
                this.parent.AddToComponents(this);
                
                if (this is IScene scene)
                {
                    scene.Fiber = this.parent.iScene.Fiber;
                    this.IScene = scene;
                }
                else
                {
                    this.IScene = this.parent.iScene;
                }
            }
        }

        public T GetParent<T>() where T : Entity
        {
            return this.Parent as T;
        }

        [BsonIgnoreIfDefault]
        [BsonDefaultValue(0L)]
        [BsonElement]
        [BsonId]
        public long Id { get; protected set; }

        [BsonIgnore]
        protected IScene iScene;

        [MemoryPackIgnore]
        [BsonIgnore]
        public IScene IScene
        {
            get
            {
                return this.iScene;
            }
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"domain cant set null: {this.GetType().FullName}");
                }

                if (this.iScene == value)
                {
                    return;
                }

                IScene preScene = this.iScene;
                this.iScene = value;

                if (preScene == null)
                {
                    if (this.InstanceId == 0)
                    {
                        this.InstanceId = IdGenerater.Instance.GenerateInstanceId();
                    }

                    this.IsRegister = true;

                    // 反序列化出来的需要设置父子关系
                    if (this.componentsDB != null)
                    {
                        foreach (Entity component in this.componentsDB)
                        {
                            component.IsComponent = true;
                            this.Components.Add(this.GetLongHashCode(component.GetType()), component);
                            component.parent = this;
                        }
                    }

                    if (this.childrenDB != null)
                    {
                        foreach (Entity child in this.childrenDB)
                        {
                            child.IsComponent = false;
                            this.Children.Add(child.Id, child);
                            child.parent = this;
                        }
                    }
                }

                // 递归设置孩子的Domain
                if (this.children != null)
                {
                    foreach (Entity entity in this.children.Values)
                    {
                        entity.IScene = this.iScene;
                    }
                }

                if (this.components != null)
                {
                    foreach (Entity component in this.components.Values)
                    {
                        component.IScene = this.iScene;
                    }
                }

                if (!this.IsCreated)
                {
                    this.IsCreated = true;
                    EntitySystemSingleton.Instance.Deserialize(this);
                }
            }
        }

        [MemoryPackInclude]
        [BsonElement("Children")]
        [BsonIgnoreIfNull]
        protected List<Entity> childrenDB;

        [BsonIgnore]
        private SortedDictionary<long, Entity> children;

        [MemoryPackIgnore]
        [BsonIgnore]
        public SortedDictionary<long, Entity> Children
        {
            get
            {
                return this.children ??= ObjectPool.Instance.Fetch<SortedDictionary<long, Entity>>();
            }
        }

        private void AddToChildren(Entity entity)
        {
            this.Children.Add(entity.Id, entity);
        }

        private void RemoveFromChildren(Entity entity)
        {
            if (this.children == null)
            {
                return;
            }

            this.children.Remove(entity.Id);

            if (this.children.Count == 0)
            {
                ObjectPool.Instance.Recycle(this.children);
                this.children = null;
            }
        }

        [MemoryPackInclude]
        [BsonElement("C")]
        [BsonIgnoreIfNull]
        protected List<Entity> componentsDB;

        [BsonIgnore]
        private SortedDictionary<long, Entity> components;

        [MemoryPackIgnore]
        [BsonIgnore]
        public SortedDictionary<long, Entity> Components
        {
            get
            {
                return this.components ??= ObjectPool.Instance.Fetch<SortedDictionary<long, Entity>>();
            }
        }

        public int ComponentsCount()
        {
            if (this.components == null)
            {
                return 0;
            }
            return this.components.Count;
        }
        
        public int ChildrenCount()
        {
            if (this.children == null)
            {
                return 0;
            }
            return this.children.Count;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsRegister = false;
            this.InstanceId = 0;

            ObjectPool objectPool = ObjectPool.Instance;
            // 清理Children
            if (this.children != null)
            {
                foreach (Entity child in this.children.Values)
                {
                    child.Dispose();
                }

                this.children.Clear();
                objectPool.Recycle(this.children);
                this.children = null;

                if (this.childrenDB != null)
                {
                    this.childrenDB.Clear();
                    // 创建的才需要回到池中,从db中不需要回收
                    if (this.IsNew)
                    {
                        objectPool.Recycle(this.childrenDB);
                        this.childrenDB = null;
                    }
                }
            }

            // 清理Component
            if (this.components != null)
            {
                foreach (var kv in this.components)
                {
                    kv.Value.Dispose();
                }

                this.components.Clear();
                objectPool.Recycle(this.components);
                this.components = null;

                // 创建的才需要回到池中,从db中不需要回收
                if (this.componentsDB != null)
                {
                    this.componentsDB.Clear();
                    if (this.IsNew)
                    {
                        objectPool.Recycle(this.componentsDB);
                        this.componentsDB = null;
                    }
                }
            }

            // 触发Destroy事件
            if (this is IDestroy)
            {
                EntitySystemSingleton.Instance.Destroy(this);
            }

            this.iScene = null;

            if (this.parent != null && !this.parent.IsDisposed)
            {
                if (this.IsComponent)
                {
                    this.parent.RemoveComponent(this);
                }
                else
                {
                    this.parent.RemoveFromChildren(this);
                }
            }

            this.parent = null;

            base.Dispose();
            
            // 把status字段其它的status标记都还原
            bool isFromPool = this.IsFromPool;
            this.status = EntityStatus.None;
            this.IsFromPool = isFromPool;
            
            ObjectPool.Instance.Recycle(this);
        }

        private void AddToComponents(Entity component)
        {
            this.Components.Add(this.GetLongHashCode(component.GetType()), component);
        }

        private void RemoveFromComponents(Entity component)
        {
            if (this.components == null)
            {
                return;
            }

            this.components.Remove(this.GetLongHashCode(component.GetType()));

            if (this.components.Count == 0)
            {
                ObjectPool.Instance.Recycle(this.components);
                this.components = null;
            }
        }

        public K GetChild<K>(long id) where K : Entity
        {
            if (this.children == null)
            {
                return null;
            }

            this.children.TryGetValue(id, out Entity child);
            return child as K;
        }

        public void RemoveChild(long id)
        {
            if (this.children == null)
            {
                return;
            }

            if (!this.children.TryGetValue(id, out Entity child))
            {
                return;
            }

            this.children.Remove(id);
            child.Dispose();
        }

        public void RemoveComponent<K>() where K : Entity
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Type type = typeof (K);
            
            Entity c;
            if (!this.components.TryGetValue(this.GetLongHashCode(type), out c))
            {
                return;
            }

            this.RemoveFromComponents(c);
            c.Dispose();
        }

        private void RemoveComponent(Entity component)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Entity c;
            if (!this.components.TryGetValue(this.GetLongHashCode(component.GetType()), out c))
            {
                return;
            }

            if (c.InstanceId != component.InstanceId)
            {
                return;
            }

            this.RemoveFromComponents(c);
            c.Dispose();
        }

        public void RemoveComponent(Type type)
        {
            if (this.IsDisposed)
            {
                return;
            }

            Entity c;
            if (!this.components.TryGetValue(this.GetLongHashCode(type), out c))
            {
                return;
            }

            RemoveFromComponents(c);
            c.Dispose();
        }

        public K GetComponent<K>() where K : Entity
        {
            if (this.components == null)
            {
                return null;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponentSys)
            {
                EntitySystemSingleton.Instance.GetComponentSys(this, typeof(K));
            }
            
            Entity component;
            if (!this.components.TryGetValue(this.GetLongHashCode(typeof (K)), out component))
            {
                return default;
            }

            return (K) component;
        }

        public Entity GetComponent(Type type)
        {
            if (this.components == null)
            {
                return null;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            // 这个要在tryget之前调用，因为有可能components没有，但是执行GetComponentSystem后又有了
            if (this is IGetComponentSys)
            {
                EntitySystemSingleton.Instance.GetComponentSys(this, type);
            }
            
            Entity component;
            if (!this.components.TryGetValue(this.GetLongHashCode(type), out component))
            {
                return null;
            }

            return component;
        }

        private static Entity Create(Type type, bool isFromPool)
        {
            Entity component;
            if (isFromPool)
            {
                component = (Entity) ObjectPool.Instance.Fetch(type);
            }
            else
            {
                component = Activator.CreateInstance(type) as Entity;
            }

            component.IsFromPool = isFromPool;
            component.IsCreated = true;
            component.IsNew = true;
            component.Id = 0;
            return component;
        }

        public Entity AddComponent(Entity component)
        {
            Type type = component.GetType();
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            return component;
        }

        public Entity AddComponent(Type type, bool isFromPool = false)
        {
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EntitySystemSingleton entitySystemSingleton = EntitySystemSingleton.Instance;
            entitySystemSingleton.Awake(component);

            return component;
        }

        public K AddComponentWithId<K>(long id, bool isFromPool = false) where K : Entity, IAwake, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton entitySystemSingleton = EntitySystemSingleton.Instance;
            entitySystemSingleton.Awake(component);

            return component as K;
        }

        public K AddComponentWithId<K, P1>(long id, P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton entitySystemSingleton = EntitySystemSingleton.Instance;
            entitySystemSingleton.Awake(component, p1);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton entitySystemSingleton = EntitySystemSingleton.Instance;
            entitySystemSingleton.Awake(component, p1, p2);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton entitySystemSingleton = EntitySystemSingleton.Instance;
            entitySystemSingleton.Awake(component, p1, p2, p3);

            return component as K;
        }

        public K AddComponent<K>(bool isFromPool = false) where K : Entity, IAwake, new()
        {
            return this.AddComponentWithId<K>(this.Id, isFromPool);
        }

        public K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            return this.AddComponentWithId<K, P1>(this.Id, p1, isFromPool);
        }

        public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            return this.AddComponentWithId<K, P1, P2>(this.Id, p1, p2, isFromPool);
        }

        public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            return this.AddComponentWithId<K, P1, P2, P3>(this.Id, p1, p2, p3, isFromPool);
        }

        public Entity AddChild(Entity entity)
        {
            entity.Parent = this;
            return entity;
        }

        public T AddChild<T>(bool isFromPool = false) where T : Entity, IAwake
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        public T AddChild<T, A>(A a, bool isFromPool = false) where T : Entity, IAwake<A>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a);
            return component;
        }

        public T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b, c);
            return component;
        }

        public T AddChildWithId<T>(long id, bool isFromPool = false) where T : Entity, IAwake
        {
            Type type = typeof (T);
            T component = Entity.Create(type, isFromPool) as T;
            component.Id = id;
            component.Parent = this;
            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        public T AddChildWithId<T, A>(long id, A a, bool isFromPool = false) where T : Entity, IAwake<A>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a);
            return component;
        }

        public T AddChildWithId<T, A, B>(long id, A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChildWithId<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b, c);
            return component;
        }

        protected virtual long GetLongHashCode(Type type)
        {
            return type.TypeHandle.Value.ToInt64();
        }

        public override void BeginInit()
        {
            EntitySystemSingleton.Instance.Serialize(this);
            
            if (!this.IsCreated) return;

            this.componentsDB?.Clear();
            if (this.components != null && this.components.Count != 0)
            {
                ObjectPool objectPool = ObjectPool.Instance;
                foreach (Entity entity in this.components.Values)
                {
                    if (entity is not ISerializeToEntity)
                    {
                        continue;
                    }

                    this.componentsDB ??= objectPool.Fetch<List<Entity>>();
                    this.componentsDB.Add(entity);

                    entity.BeginInit();
                }
            }

            this.childrenDB?.Clear();
            if (this.children != null && this.children.Count != 0)
            {
                ObjectPool objectPool = ObjectPool.Instance;
                foreach (Entity entity in this.children.Values)
                {
                    if (entity is not ISerializeToEntity)
                    {
                        continue;
                    }

                    this.childrenDB ??= objectPool.Fetch<List<Entity>>();
                    this.childrenDB.Add(entity);

                    entity.BeginInit();
                }
            }
        }
    }
}