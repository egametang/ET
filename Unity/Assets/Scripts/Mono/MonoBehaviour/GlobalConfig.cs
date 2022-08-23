using UnityEngine;

namespace ET
{
    // 1 mono模式 2 mono热重载模式 3 Codes模式，将逻辑代码作为编辑器dll，编辑器可以调用到逻辑代码
    public enum LoadMode
    {
        Mono = 1,
        Reload = 2,
    }

    public enum CodeMode
    {
        Client = 1,
        Server = 2,
        ClientServer = 3,
    }
    
    [CreateAssetMenu(menuName = "ET/CreateGlobalConfig", fileName = "GlobalConfig", order = 0)]
    public class GlobalConfig: ScriptableObject
    {
        public LoadMode LoadMode;

        public CodeMode CodeMode;
    }
}