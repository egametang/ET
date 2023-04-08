using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [EnableMethod]
    [ChildOf]
    public class LSScene: LSEntity
    {
        public int Zone
        {
            get;
        }

        public SceneType SceneType
        {
            get;
        }

        public string Name
        {
            get;
        }

        public LSScene(long instanceId, int zone, SceneType sceneType, string name, Entity parent)
        {
            this.Id = instanceId;
            this.InstanceId = instanceId;
            this.Zone = zone;
            this.SceneType = sceneType;
            this.Name = name;
            this.IsCreated = true;
            this.IsNew = true;
            this.Parent = parent;
            this.Domain = this;
            this.IsRegister = true;
            Log.Info($"scene create: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        public LSScene(long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent)
        {
            this.Id = id;
            this.InstanceId = instanceId;
            this.Zone = zone;
            this.SceneType = sceneType;
            this.Name = name;
            this.IsCreated = true;
            this.IsNew = true;
            this.Parent = parent;
            this.Domain = this;
            this.IsRegister = true;
            Log.Info($"scene create: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            Log.Info($"scene dispose: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        [BsonIgnore]
        public new Entity Domain
        {
            get => this.domain;
            private set => this.domain = value;
        }

        [BsonIgnore]
        public new Entity Parent
        {
            get
            {
                return this.parent;
            }
            private set
            {
                if (value == null)
                {
                    //this.parent = this;
                    return;
                }

                this.parent = value;
                this.parent.Children.Add(this.Id, this);
            }
        }

        [BsonElement]
        private long idGenerator;

        public long GetId()
        {
            return ++this.idGenerator;
        }

        public FixedUpdater FixedUpdater { get; set; }
    }

    public static class LSSceneHelper
    {
        public static LSScene DomainScene(this LSEntity entity)
        {
            return entity.Domain as LSScene;
        }
        
        public static long GetId(this LSEntity entity)
        {
            return entity.DomainScene().GetId();
        }
    }
}