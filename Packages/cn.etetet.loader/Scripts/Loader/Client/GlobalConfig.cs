using UnityEngine;

namespace ET
{
    public enum CodeMode
    {
        Client = 1,
        Server = 2,
        ClientServer = 3,
    }
    
    public enum BuildType
    {
        Debug,
        Release,
    }
    
    [CreateAssetMenu(menuName = "ET/CreateGlobalConfig", fileName = "GlobalConfig", order = 0)]
    public class GlobalConfig: ScriptableObject
    {
        public CodeMode CodeMode;

        public string SceneName;

        public string Address;
    }
}