using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework
{
    internal static partial class LoadHelper
    {
        private static Dictionary<Object, LoadHandle> m_ObjLoadHandle = new Dictionary<Object, LoadHandle>();

        internal static bool AddLoadHandle(Object obj, LoadHandle handle)
        {
            if (m_ObjLoadHandle.ContainsKey(obj))
            {
                Debug.LogError($"此obj {obj.name} Handle 已存在 请检查 请勿创建多个");
                return false;
            }

            m_ObjLoadHandle.Add(obj, handle);
            return true;
        }

        private static bool RemoveLoadHandle(LoadHandle handle)
        {
            var obj = handle.Object;
            if (obj == null)
            {
                return false;
            }

            return RemoveLoadHandle(obj);
        }

        private static bool RemoveLoadHandle(Object obj)
        {
            if (!m_ObjLoadHandle.ContainsKey(obj))
            {
                Debug.LogError($"此obj {obj.name} Handle 不存在 请检查 请先创建设置");
                return false;
            }

            return m_ObjLoadHandle.Remove(obj);
        }

        internal static LoadHandle GetLoadHandle(Object obj)
        {
            if (!m_ObjLoadHandle.ContainsKey(obj))
            {
                Debug.LogError($"此obj {obj.name} Handle 不存在 请检查 请先创建设置");
                return null;
            }

            return m_ObjLoadHandle[obj];
        }
    }
}