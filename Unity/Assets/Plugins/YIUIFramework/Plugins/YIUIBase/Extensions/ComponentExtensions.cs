using UnityEngine;
using UnityEngine.Assertions;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="UnityEngine.Component"/>.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// 从目标组件中获取一个组件，如果是组件类型不存在，则添加
        /// </summary>
        public static Component GetOrAddComponent(
            this Component obj, System.Type type)
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
            this Component obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.gameObject.AddComponent<T>();
            }

            return component;
        }

        /// <summary>
        /// 从目标组件中获取一个组件，如果是组件类型不存在，则添加
        /// 标记不保存
        /// </summary>
        public static T GetOrAddComponentDontSave<T>(
            this Component obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component           = obj.gameObject.AddComponent<T>();
                component.hideFlags = HideFlags.DontSave;
            }

            return component;
        }

        /// <summary>
        /// 检查目标组件的GameObject上是否有一个或多个指定类型的组件
        /// </summary>
        public static bool HasComponent(
            this Component obj, System.Type type)
        {
            return obj.GetComponent(type) != null;
        }

        /// <summary>
        /// 检查目标组件的GameObject上是否有一个或多个指定类型的组件
        /// </summary>
        public static bool HasComponent<T>(
            this Component obj) where T : Component
        {
            return obj.GetComponent<T>() != null;
        }

        /// <summary>
        /// 在parent中查找组件，即使该组件处于非活动状态或已禁用 都能查询
        /// </summary>
        public static T GetComponentInParentHard<T>(
            this Component obj) where T : Component
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