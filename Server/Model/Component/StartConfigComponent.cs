using System;
using System.Collections.Generic;
using System.Net;

namespace ETModel
{
    [ObjectSystem]
    public class StartConfigComponentSystem: AwakeSystem<StartConfigComponent, StartConfig, long>
    {
        public override void Awake(StartConfigComponent self, StartConfig allConfig, long id)
        {
            self.Awake(allConfig, id);
        }
    }

    public class StartConfigComponent: Entity
    {
        public static StartConfigComponent Instance { get; private set; }

        // 所有进程的配置
        public StartConfig AllConfig;

        // 当前进程的配置
        public StartConfig StartConfig;
        
        // 所有domain的字典
        private Dictionary<long, StartConfig> configDict = new Dictionary<long, StartConfig>();

        // 所有有内网地址的进程字典
        private Dictionary<long, string> innerAddressDict = new Dictionary<long, string>();
        
        // 所有agent
        public Dictionary<long, StartConfig> Agents = new Dictionary<long, StartConfig>();
        
        private Dictionary<string, StartConfig> nameDict = new Dictionary<string, StartConfig>();
        
        private Dictionary<int, StartConfig> typeDict = new Dictionary<int, StartConfig>();

        public List<StartConfig> Gates { get; } =  new List<StartConfig>();
        
        public void Awake(StartConfig allConfig, long id)
        {
            Instance = this;
            this.AllConfig = allConfig;
            this.AllConfig.Parent = this;

            // 每个进程的配置
            foreach (StartConfig s in this.AllConfig.List)
            {
                s.SceneInstanceId = (s.Id << IdGenerater.HeadPos) + s.Id;

                if (s.Id == id)
                {
                    this.StartConfig = s;
                }
                
                InnerConfig innerConfig = s.GetComponent<InnerConfig>();
                if (innerConfig != null)
                {
                    this.innerAddressDict.Add(s.Id, innerConfig.Address);
                }
                
                // 每个进程里面domain的配置
                foreach (StartConfig startConfig in s.List)
                {
                    startConfig.SceneInstanceId = (startConfig.Parent.Id << IdGenerater.HeadPos) + startConfig.Id;
                    
                    this.configDict.Add(startConfig.Id, startConfig);

                    SceneConfig sceneConfig = startConfig.GetComponent<SceneConfig>();

                    switch (sceneConfig.SceneType)
                    {
                        case SceneType.Gate:
                            this.Gates.Add(startConfig);
                            break;
                        case SceneType.Map:
                            this.nameDict.Add(sceneConfig.Name, startConfig);
                            break;
                        default:
                            this.typeDict.Add((int)sceneConfig.SceneType, startConfig);
                            break;
                    }
                }
            }
        }
        
        public long GetInstanceId(SceneType sceneType)
        {
            if (!this.typeDict.TryGetValue((int) sceneType, out StartConfig startConfig))
            {
                throw new Exception($"GetInstanceId cant get StartConfig: {sceneType}");
            }
            return startConfig.SceneInstanceId;
        }
        
        public StartConfig GetByType(SceneType sceneType)
        {
            if (!this.typeDict.TryGetValue((int) sceneType, out StartConfig startConfig))
            {
                throw new Exception($"GetByType cant get StartConfig: {sceneType}");
            }
            return startConfig;
        }

        public StartConfig GetByName(string sceneName)
        {
            if (!this.nameDict.TryGetValue(sceneName, out StartConfig startConfig))
            {
                throw new Exception($"GetByName cant get StartConfig: {sceneName}");
            }
            return startConfig;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            Instance = null;
            
            this.configDict.Clear();
            this.innerAddressDict.Clear();
            this.Gates.Clear();
            this.Agents.Clear();
            this.typeDict.Clear();
            this.nameDict.Clear();
            this.StartConfig = null;
        }

        public StartConfig GetAgent(long agentId)
        {
            return this.Agents[agentId];
        }

        public StartConfig Get(long id)
        {
            try
            {
                return this.configDict[id];
            }
            catch (Exception e)
            {
                throw new Exception($"not found startconfig: {id}", e);
            }
        }
        
        public string GetProcessInnerAddress(long id)
        {
            try
            {
                // 内网地址需要找到进程配置，进程配置是domain配置的parent
                return this.innerAddressDict[id];
            }
            catch (Exception e)
            {
                throw new Exception($"not found innerAddress: {id}", e);
            }
        }

        public StartConfig[] GetAll()
        {
            List<StartConfig> startConfigs = new List<StartConfig>();
            foreach (var kv in this.AllConfig.Children)
            {
                startConfigs.Add((StartConfig)kv.Value);
            }
            return startConfigs.ToArray();
        }
    }
}