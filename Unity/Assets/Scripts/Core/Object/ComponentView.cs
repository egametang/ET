#if ENABLE_CODES
using UnityEngine;

namespace ET
{
    public class ComponentView: MonoBehaviour
    {
        public Entity Component
        {
            get;
            set;
        }
    }
}
#endif