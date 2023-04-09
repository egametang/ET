using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

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
    }

    [EnableMethod]
    [ChildOf]
    public class LSScene: LSEntity, IDeserialize
    {
        public LSScene(long id)
        {
            this.Id = id;
            this.InstanceId = IdGenerater.Instance.GenerateInstanceId();
            this.IsCreated = true;
            this.IsNew = true;
            this.IsRegister = true;
            Log.Info($"LSScene create: {this.Id} {this.InstanceId}");
        }
        
        [BsonIgnore]
        public new Entity Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                value?.AddChild(this);
                this.Domain = this;
            }
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
    }
}