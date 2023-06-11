using UnityEngine;

namespace ET
{
    public class GlobalComponent: ProcessSingleton<GlobalComponent>, ISingletonAwake
    {
        public Transform Global;
        public Transform Unit { get; set; }
        public Transform UI;

        public GlobalConfig GlobalConfig { get; set; }
        
        public void Awake()
        {
            this.Global = GameObject.Find("/Global").transform;
            this.Unit = GameObject.Find("/Global/Unit").transform;
            this.UI = GameObject.Find("/Global/UI").transform;
            this.GlobalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
        }
    }
}