using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using TrueSync;

namespace ET
{
    public static class LSSceneSystem
    {
        [ObjectSystem]
        public class LSSceneAwakeSystem: AwakeSystem<LSWorld>
        {
            protected override void Awake(LSWorld self)
            {
                self.Updater.Parent = self;
            }
        }
        
        public class DeserializeSystem: DeserializeSystem<LSWorld>
        {
            protected override void Deserialize(LSWorld self)
            {
                self.Updater.Parent = self;
            }
        }

        public static LSWorld DomainScene(this LSEntity entity)
        {
            return entity.Domain as LSWorld;
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
    public class LSWorld: LSEntity, IAwake, IScene, IDeserialize
    {
        public LSWorld()
        {
        }
        
        public LSWorld(SceneType sceneType)
        {
            this.Id = this.GetId();

            this.SceneType = sceneType;
            
            this.Updater.Parent = this;
            
            Log.Info($"LSScene create: {this.Id} {this.InstanceId}");
        }
        
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

        public void Add(LSEntity entity) 
        {
            this.allLSEntities.Add(entity.Id, entity);

            TypeSystems.OneTypeSystems oneTypeSystems = LSSington.Instance.GetOneTypeSystems(entity.GetType());
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
        
        public SceneType SceneType { get; set; }
    }
}