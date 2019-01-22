using UnityEngine;

namespace ETHotfix
{
    public static class GameObjectHelper
    {
        public static T Ensure<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            var t = gameObject.GetComponent<T>();

            if (!t)
            {
                t = gameObject.AddComponent<T>();
            }

            return t;
        }
    }
}
