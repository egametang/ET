using UnityEngine;
using YooAsset;

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
        /// <summary>
        /// 代码类型(客户端/服务端/双端)
        /// </summary>
        public CodeMode CodeMode;

        /// <summary>
        /// 是否启用Dll
        /// </summary>
        public bool EnableDll;

        /// <summary>
        /// 打包类型（Develop/Release）
        /// </summary>
        public BuildType BuildType;

        /// <summary>
        /// App类型（状态同步/帧同步）
        /// </summary>
        public AppType AppType;

        /// <summary>
        /// 运行模式
        /// </summary>
        public EPlayMode EPlayMode;
    }
}