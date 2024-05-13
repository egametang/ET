using System.Collections.Generic;

namespace ET.Server
{
    [ConfigProcess]
    public class LocationConfigSingleton: Singleton<LocationConfigSingleton>, ISingletonAwake
    {
        private readonly List<StartSceneConfig> locations = new();
        
        public void Awake()
        {
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.GetAll().Values)
            {
                if (startSceneConfig.Type == SceneType.Location)
                {
                    this.locations.Add(startSceneConfig);
                }
            }
        }

        public StartSceneConfig GetLocation(long key)
        {
            return this.locations[(int)((ulong)key % (ulong)this.locations.Count)];
        }
    }
}