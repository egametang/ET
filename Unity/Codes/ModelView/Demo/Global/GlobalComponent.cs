using UnityEngine;

namespace ET
{
    public class GlobalComponent: Entity, IAwake
    {
        public static GlobalComponent Instance;
        
        public Transform Global { get; set; }
        public Transform Unit { get; set; }
        public Transform UI { get; set; }
    }
}