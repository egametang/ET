using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// ZoneScene 管理者组件
    /// </summary>
    public class ZoneSceneManagerComponent: Entity, IAwake, IDestroy
    {
        public static ZoneSceneManagerComponent Instance;
        public Dictionary<int, Scene> ZoneScenes = new Dictionary<int, Scene>();
    }
}