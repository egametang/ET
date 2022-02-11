using UnityEngine;

namespace ET
{
    /// <summary>
    /// 全局组件
    /// </summary>
    public class GlobalComponent: Entity, IAwake
    {
        public static GlobalComponent Instance;
        
        public Transform Global;
        public Transform Unit;
        public Transform UI;
    }
}