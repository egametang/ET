using UnityEngine;

namespace ET
{
#if !NOT_UNITY
    public class ComponentView: MonoBehaviour
    {
        public object Component
        {
            get;
            set;
        }
    }
#endif
}