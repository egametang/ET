using UnityEngine;
using UnityEngine.Assertions;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="GameObject"/>.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 从目标组件中获取一个组件，如果是组件类型不存在，则添加
        /// </summary>
        public static Component GetOrAddComponent(
            this GameObject obj, System.Type type)
        {
            var component = obj.GetComponent(type);
            if (component == null)
            {
                component = obj.gameObject.AddComponent(type);
            }

            return component;
        }

        /// <summary>
        /// 从目标组件中获取一个组件，如果是组件类型不存在，则添加
        /// </summary>
        public static T GetOrAddComponent<T>(
            this GameObject obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.gameObject.AddComponent<T>();
            }

            return component;
        }

        /// <summary>
        /// 检查目标组件上是否有一个或多个特定类型的组件
        /// </summary>
        public static bool HasComponent(
            this GameObject obj, System.Type type)
        {
            return obj.GetComponent(type) != null;
        }

        /// <summary>
        /// 检查目标组件上是否有一个或多个特定类型的组件
        /// </summary>
        public static bool HasComponent<T>(
            this GameObject obj) where T : Component
        {
            return obj.GetComponent<T>() != null;
        }

        /// <summary>
        /// 为GameObject及其所有子对象设置图层。
        /// </summary>
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform t in obj.transform)
            {
                t.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// 在parent中查找组件，即使该组件处于非活动状态或禁用状态。
        /// </summary>
        public static T GetComponentInParentHard<T>(
            this GameObject obj) where T : Component
        {
            Assert.IsNotNull(obj);
            var transform = obj.transform;
            while (transform != null)
            {
                var component = transform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                transform = transform.parent;
            }

            return null;
        }
    }
}