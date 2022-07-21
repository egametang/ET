using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace ET.StartServer
{
    public partial class TbStartScene
    {
        public MultiMap<int, StartScene> Gates = new MultiMap<int, StartScene>();
        
        public MultiMap<int, StartScene> ProcessScenes = new MultiMap<int, StartScene>();
        
        public Dictionary<long, Dictionary<string, StartScene>> ZoneScenesByName = new Dictionary<long, Dictionary<string, StartScene>>();

        public StartScene LocationConfig;
        
        public List<StartScene> Robots = new List<StartScene>();
        
        public List<StartScene> GetByProcess(int process)
        {
            return this.ProcessScenes[process];
        }
        
        public StartScene GetBySceneName(int zone, string name)
        {
            return this.ZoneScenesByName[zone][name];
        }
        
        partial void PostInit()
        {
            foreach (StartScene StartScene in DataMap.Values)
            {
                this.ProcessScenes.Add(StartScene.Process, StartScene);
                
                if (!this.ZoneScenesByName.ContainsKey(StartScene.Zone))
                {
                    this.ZoneScenesByName.Add(StartScene.Zone, new Dictionary<string, StartScene>());
                }
                this.ZoneScenesByName[StartScene.Zone].Add(StartScene.Name, StartScene);
                
                switch (StartScene.SceneType)
                {
                    case SceneType.Gate:
                        this.Gates.Add(StartScene.Zone, StartScene);
                        break;
                    case SceneType.Location:
                        this.LocationConfig = StartScene;
                        break;
                    case SceneType.Robot:
                        this.Robots.Add(StartScene);
                        break;
                }
            }
        }
    }
    
    public partial class StartScene
    {
        public long InstanceId;

        public StartProcess StartProcessConfig
        {
            get
            {
                return Tables.Ins.TbStartProcess.Get(this.Process);
            }
        }
        
        public StartZone StartZoneConfig
        {
            get
            {
                return Tables.Ins.TbStartZone.Get(this.Zone);
            }
        }

        // 内网地址外网端口，通过防火墙映射端口过来
        private IPEndPoint innerIPOutPort;

        public IPEndPoint InnerIPOutPort
        {
            get
            {
                if (innerIPOutPort == null)
                {
                    this.innerIPOutPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.InnerIP}:{this.OuterPort}");
                }

                return this.innerIPOutPort;
            }
        }

        private IPEndPoint outerIPPort;

        // 外网地址外网端口
        public IPEndPoint OuterIPPort
        {
            get
            {
                if (this.outerIPPort == null)
                {
                    this.outerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.OuterIP}:{this.OuterPort}");
                }

                return this.outerIPPort;
            }
        }

        partial void PostInit()
        {
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.Process, (uint) this.Id);
            this.InstanceId = instanceIdStruct.ToLong();
        }
    }
}