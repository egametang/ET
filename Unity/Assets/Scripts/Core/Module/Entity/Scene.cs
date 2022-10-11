using System.Diagnostics;

namespace ET
{
    [EnableMethod]
    [DebuggerDisplay("ViewName,nq")]
    [ChildOf]
    public sealed class Scene: Entity
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

        public Scene(long instanceId, int zone, SceneType sceneType, string name, Entity parent)
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

        public Scene(long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent)
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

        public new Entity Domain
        {
            get => this.domain;
            private set => this.domain = value;
        }

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
        
        protected override string ViewName
        {
            get
            {
                return $"{this.GetType().Name} ({this.SceneType})";    
            }
        }
    }
}