using System.Collections.Generic;
using System.ComponentModel;

namespace ET
{
    public partial class StartSceneConfigCategory
    {
        public MultiMap<int, StartSceneConfig> Gates = new MultiMap<int, StartSceneConfig>();
        
        public MultiMap<int, StartSceneConfig> ProcessScenes = new MultiMap<int, StartSceneConfig>();
        
        public Dictionary<long, Dictionary<string, StartSceneConfig>> ZoneScenesByName = new Dictionary<long, Dictionary<string, StartSceneConfig>>();

        public StartSceneConfig LocationConfig;
        
        public List<StartSceneConfig> GetByProcess(int process)
        {
            return this.ProcessScenes[process];
        }
        
        public StartSceneConfig GetBySceneName(int zone, string name)
        {
            return this.ZoneScenesByName[zone][name];
        }
        
        public override void EndInit()
        {
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values)
            {
                this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig);
                
                if (!this.ZoneScenesByName.ContainsKey(startSceneConfig.Zone))
                {
                    this.ZoneScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.ZoneScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);
                
                switch (startSceneConfig.Type)
                {
                    case SceneType.Gate:
                        this.Gates.Add(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.Location:
                        this.LocationConfig = startSceneConfig;
                        break;
                }
            }
        }
    }
    
    public partial class StartSceneConfig: ISupportInitialize
    {
        public long SceneId;
        
        public SceneType Type;

        public StartProcessConfig StartProcessConfig
        {
            get
            {
                return StartProcessConfigCategory.Instance.Get(this.Process);
            }
        }
        
        public StartZoneConfig StartZoneConfig
        {
            get
            {
                return StartZoneConfigCategory.Instance.Get(this.Process);
            }
        }

        private string outerAddress;
        
        public string OuterAddress
        {
            get
            {
                if (this.outerAddress == null)
                {
                    this.outerAddress = $"{this.StartProcessConfig.OuterIP}:{this.OuterPort}";
                }
                return this.outerAddress;
            }
        }
        
        private string innerOuterAddress;
        
        public string InnerOuterAddress
        {
            get
            {
                if (this.innerOuterAddress == null)
                {
                    this.innerOuterAddress = $"{this.StartProcessConfig.InnerAddress}:{this.OuterPort}";
                }
                return this.innerOuterAddress;
            }
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            this.Type = EnumHelper.FromString<SceneType>(this.SceneType);
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.Process, (uint) this.Id);
            this.SceneId = instanceIdStruct.ToLong();
        }
    }
}