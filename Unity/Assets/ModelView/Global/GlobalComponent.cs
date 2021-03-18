using UnityEngine;

namespace ET
{
    public class GlobalComponent: Entity
    {
        public static GlobalComponent Instance;
        
        public Transform Global;
        public Transform Unit;
        public Transform UI;
    }
}