using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [EnableMethod]
    [DebuggerDisplay("ViewName,nq")]
    [ChildOf]
    public class Scene: Entity
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

        public Scene(long instanceId, int zone, SceneType sceneType, string name)
        {
            this.Id = instanceId;
            this.InstanceId = instanceId;
            this.Zone = zone;
            this.SceneType = sceneType;
            this.Name = name;
            this.IsCreated = true;
            this.IsNew = true;
            this.IsRegister = true;
            Log.Info($"scene create: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        public Scene(long id, long instanceId, int zone, SceneType sceneType, string name)
        {
            this.Id = id;
            this.InstanceId = instanceId;
            this.Zone = zone;
            this.SceneType = sceneType;
            this.Name = name;
            this.IsCreated = true;
            this.IsNew = true;
            this.IsRegister = true;
            Log.Info($"scene create: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            Log.Info($"scene dispose: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
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
        
        protected override string ViewName
        {
            get
            {
                return $"{this.GetType().Name} ({this.SceneType})";    
            }
        }
    }
}