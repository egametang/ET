using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using TrueSync;

namespace ET
{
    public static class LSSceneSystem
    {
        public class DeserializeSystem: DeserializeSystem<LSScene>
        {
            protected override void Deserialize(LSScene self)
            {
                self.Updater.Parent = self;
            }
        }

        public static LSScene DomainScene(this LSEntity entity)
        {
            return entity.Domain as LSScene;
        }

        public static long GetId(this LSEntity entity)
        {
            return entity.DomainScene().GetId();
        }
        
        public static TSRandom GetRandom(this LSEntity entity)
        {
            return entity.DomainScene().Random;
        }
    }

    [EnableMethod]
    [ChildOf]
    public class LSScene: Scene, IDeserialize
    {
        public LSScene()
        {
        }
        
        public LSScene(long id, int zone, SceneType sceneType, string name): base(id, IdGenerater.Instance.GenerateInstanceId(), zone, sceneType, name)
        {
            this.Updater.Parent = this;
            
            Log.Info($"LSScene create: {this.Id} {this.InstanceId}");
        }

        #region AddComponent And AddChild

        public new K AddComponent<K>(bool isFromPool = false) where K : LSEntity, IAwake, new()
        {
            return this.AddComponentWithId<K>(this.GetId(), isFromPool);
        }

        public new K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : LSEntity, IAwake<P1>, new()
        {
            return this.AddComponentWithId<K, P1>(this.GetId(), p1, isFromPool);
        }

        public new K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : LSEntity, IAwake<P1, P2>, new()
        {
            return this.AddComponentWithId<K, P1, P2>(this.GetId(), p1, p2, isFromPool);
        }

        public new K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : LSEntity, IAwake<P1, P2, P3>, new()
        {
            return this.AddComponentWithId<K, P1, P2, P3>(this.GetId(), p1, p2, p3, isFromPool);
        }

        public new T AddChild<T>(bool isFromPool = false) where T : LSEntity, IAwake
        {
            return this.AddChildWithId<T>(this.GetId(), isFromPool);
        }

        public new T AddChild<T, A>(A a, bool isFromPool = false) where T : LSEntity, IAwake<A>
        {
            return this.AddChildWithId<T, A>(this.GetId(), a, isFromPool);
        }

        public new T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : LSEntity, IAwake<A, B>
        {
            return this.AddChildWithId<T, A, B>(this.GetId(), a, b, isFromPool);
        }

        public new T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : LSEntity, IAwake<A, B, C>
        {
            return this.AddChildWithId<T, A, B, C>(this.GetId(), a, b, c, isFromPool);
        }

        #endregion
        
        private readonly Dictionary<long, LSEntity> allLSEntities = new();

        [BsonElement]
        public LSUpdater Updater = new();

        public LSEntity Get(long id)
        {
            this.allLSEntities.TryGetValue(id, out LSEntity entity);
            return entity;
        }

        public void Remove(long id)
        {
            this.allLSEntities.Remove(id);
        }

        public void RegisterSystem(LSEntity entity)
        {
            this.allLSEntities.Add(entity.Id, entity);

            Type type = entity.GetType();

            TypeSystems.OneTypeSystems oneTypeSystems = LSSington.Instance.TypeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }

            if (oneTypeSystems.QueueFlag[LSQueneUpdateIndex.LSUpdate])
            {
                this.Updater.Add(entity);
            }
        }

        [BsonElement]
        private long idGenerator;

        public long GetId()
        {
            return ++this.idGenerator;
        }

        public TSRandom Random { get; set; }
    }
}