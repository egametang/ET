using System.Diagnostics;

namespace ET
{
    [EnableMethod]
    [ChildOf]
    public class Scene: Entity, IScene
    {
        public int Zone
        {
            get;
        }

        public SceneType SceneType
        {
            get;
            set;
        }

        public string Name
        {
            get;
        }

        public Scene(VProcess vProcess)
        {
            this.VProcess = vProcess;
        }

        public Scene(VProcess vProcess, long id, long instanceId, int zone, SceneType sceneType, string name)
        {
            this.VProcess = vProcess;
            this.Id = id;
            this.InstanceId = instanceId;
            this.Zone = zone;
            this.SceneType = sceneType;
            this.Name = name;
            this.IsCreated = true;
            this.IsNew = true;
            this.IsRegister = true;
            this.IScene = this;
            Log.Info($"scene create: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            Log.Info($"scene dispose: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
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