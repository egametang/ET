using System.Collections.Generic;

namespace ET.Server
{
    [ConfigProcess]
    public class RouterConfigSingleton: Singleton<RouterConfigSingleton>, ISingletonAwake
    {
        private readonly List<StartSceneConfig> routers = new();
        
        public void Awake()
        {
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.GetAll().Values)
            {
                if (startSceneConfig.Type == SceneType.Router)
                {
                    this.routers.Add(startSceneConfig);
                }
            }
        }

        public List<StartSceneConfig> GetRouters()
        {
            return this.routers;
        }
    }
}