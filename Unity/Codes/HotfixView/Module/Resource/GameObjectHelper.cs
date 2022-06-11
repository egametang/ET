using System;
using UnityEngine;

namespace ET
{
    public static class GameObjectHelper
    {
        public static T Get<T>(this GameObject gameObject, string key) where T : class
        {
            try
            {
                return gameObject.GetComponent<ReferenceCollector>().Get<T>(key);
            }
            catch (Exception e)
            {
                throw new Exception($"获取{gameObject.name}的ReferenceCollector key失败, key: {key}", e);
            }
        }
        public static T GetComponentFormRC<T>(this GameObject gameObject, string key) where T : Component
        {
            var gob = gameObject.Get<GameObject>(key);
            if (gob == null)
            {
                Debug.LogWarning($"{gameObject.name}找不到物体{key}");
                return null;
            }
            var com = gob.GetComponent<T>();
            if (com == null)
            {
                Debug.LogWarning($"{gameObject.name}的{key}物体找不到{typeof(T).Name}组件");
            }
            return com;
        }
    }
}