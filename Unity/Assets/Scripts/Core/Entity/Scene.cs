using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [EnableMethod]
    [ChildOf]
    public class Scene: Entity, IScene
    {
        [BsonIgnore]
        public Fiber Fiber { get; set; }
        
        public string Name { get; }
        
        public SceneType SceneType
        {
            get;
            set;
        }

        public Scene()
        {
        }

        public Scene(Fiber fiber, long id, long instanceId, SceneType sceneType, string name)
        {
            this.Id = id;
            this.Name = name;
            this.InstanceId = instanceId;
            this.SceneType = sceneType;
            this.IsCreated = true;
            this.IsNew = true;
            this.Fiber = fiber;
            this.IScene = this;
            this.IsRegister = true;
            Log.Info($"scene create: {this.SceneType} {this.Id} {this.InstanceId}");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            Log.Info($"scene dispose: {this.SceneType} {this.Id} {this.InstanceId}");
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