using UnityEngine;

namespace ET
{
#if !NOT_CLIENT
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