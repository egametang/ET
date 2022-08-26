#if ENABLE_VIEW
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