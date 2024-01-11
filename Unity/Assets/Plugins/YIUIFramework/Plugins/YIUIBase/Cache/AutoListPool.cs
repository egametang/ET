using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YIUIFramework
{
    /// <summary>
    /// 可以自动回收的池
    /// 适合用于临时的函数返对象
    /// </summary>
    public static class AutoListPool<T>
    {
        private static ListPool g_pool = new ListPool();

        static AutoListPool()
        {
            AutoListPool.g_recyclers.Add(g_pool);
        }

        public static List<T> Get()
        {
            return g_pool.Get();
        }

        private class ListPool : Recycler
        {
            private List<List<T>> m_pool = new List<List<T>>(5);
            private int           m_index;

            public List<T> Get()
            {
                List<T> item;
                if (m_index == m_pool.Count)
                {
                    item = new List<T>();
                    m_pool.Add(item);
                }
                else
                {
                    item = m_pool[m_index];
                }

                m_index++;
                Dirty                = true;
                AutoListPool.g_dirty = true;

                //虽然回收时已经清了，但这样双保险，不香么？
                item.Clear();
                return item;
            }

            public override void Recycle()
            {
                if (m_index == 0)
                {
                    return;
                }

                for (int i = 0; i < m_pool.Count; i++)
                {
                    m_pool[i].Clear();
                }

                m_index = 0;
                Dirty   = false;
            }
        }
    }

    internal abstract class Recycler
    {
        public          bool Dirty;
        public abstract void Recycle();
    }

    [DefaultExecutionOrder(int.MaxValue)]
    internal class AutoListPool : MonoBehaviour
    {
        internal static readonly List<Recycler> g_recyclers = new List<Recycler>();
        internal static          bool           g_dirty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnLoad()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedHandler;
            SceneManager.sceneLoaded += OnSceneLoadedHandler;
        }

        private static void OnSceneLoadedHandler(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1)
        {
            var autoListPoolGo = new GameObject("AutoListPool");
            autoListPoolGo.hideFlags = HideFlags.HideInHierarchy;
            autoListPoolGo.AddComponent<AutoListPool>();
        }

        private void LateUpdate()
        {
            if (!g_dirty)
            {
                return;
            }

            g_dirty = false;
            for (int i = 0; i < g_recyclers.Count; i++)
            {
                var item = g_recyclers[i];
                if (item.Dirty)
                {
                    item.Recycle();
                }
            }
        }
    }
}