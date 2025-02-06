using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonoHook
{
    /// <summary>
    /// Hook 池，防止重复 Hook
    /// </summary>
    public static class HookPool
    {
        private static Dictionary<MethodBase, MethodHook> _hooks = new Dictionary<MethodBase, MethodHook>();

        public static void AddHook(MethodBase method, MethodHook hook)
        {
            MethodHook preHook;
            if (_hooks.TryGetValue(method, out preHook))
            {
                preHook.Uninstall();
                _hooks[method] = hook;
            }
            else
                _hooks.Add(method, hook);
        }

        public static MethodHook GetHook(MethodBase method)
        {
            if (method == null) return null;

            MethodHook hook;
            if (_hooks.TryGetValue(method, out hook))
                return hook;
            return null;
        }

        public static void RemoveHooker(MethodBase method)
        {
            if (method == null) return;

            _hooks.Remove(method);
        }

        public static void UninstallAll()
        {
            var list = _hooks.Values.ToList();
            foreach (var hook in list)
                hook.Uninstall();

            _hooks.Clear();
        }

        public static void UninstallByTag(string tag)
        {
            var list = _hooks.Values.ToList();
            foreach (var hook in list)
            {
                if(hook.tag == tag)
                    hook.Uninstall();
            }
        }

        public static List<MethodHook> GetAllHooks()
        {
            return _hooks.Values.ToList();
        }
    }

}
