//#define YIUIMACRO_SIMULATE_NONEEDITOR //模拟非编辑器状态  在编辑器使用 非编辑器加载模式 用于在编辑器下测试  

using System;
using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// UI关联帮助类
    /// </summary>
    public static class YIUIBindHelper
    {
        /// <summary>
        /// 根据创建时的类获取
        /// type 限制为 ui的component
        /// </summary>
        private static Dictionary<Type, YIUIBindVo> g_UITypeToPkgInfo = new Dictionary<Type, YIUIBindVo>();

        /// <summary>
        /// 根据 pkg + res 双字典获取
        /// </summary>
        private static Dictionary<string, Dictionary<string, YIUIBindVo>> g_UIPathToPkgInfo =
                new Dictionary<string, Dictionary<string, YIUIBindVo>>();

        /// <summary>
        /// 因为使用yooasset 规定所有资源唯一
        /// 所以这里可以抛弃pkg+res 直接使用res 可以拿到对应的资源
        /// 如果你的不是唯一的 请删除这个方法不要使用
        /// </summary>
        private static Dictionary<string, YIUIBindVo> g_UIToPkgInfo = new Dictionary<string, YIUIBindVo>();

        //改为dll过后 提供给外部的方法
        //1 从UI工具中自动生成绑定代码
        //2 外部请直接调用此方法 YIUIBindHelper.InternalGameGetUIBindVoFunc = YIUICodeGenerated.YIUIBindProvider.Get;
        public static Func<YIUIBindVo[]> InternalGameGetUIBindVoFunc { internal get; set; }

        //初始化记录
        public static bool IsInit { get; private set; }

        /// <summary>
        /// 初始化获取到所有UI相关的绑定关系
        /// Editor下是反射
        /// 其他 是序列化的文件 打包的时候一定要生成一次文件
        /// </summary>
        internal static bool InitAllBind()
        {
            if (IsInit)
            {
                Debug.LogError($"已经初始化过了 请检查");
                return false;
            }

            #if !UNITY_EDITOR || YIUIMACRO_SIMULATE_NONEEDITOR || ENABLE_DLL
            if (InternalGameGetUIBindVoFunc == null)
            {
                Debug.LogError($"使用非反射注册绑定 但是方法未实现 请检查");
                return false;
            }
            var binds = InternalGameGetUIBindVoFunc?.Invoke();
            #else
            var binds = new YIUIBindProvider().Get();
            #endif

            if (binds == null || binds.Length <= 0)
            {
                //如果才接入框架 第一个UI都没有生成是无法运行的 先生成一个UI吧
                Debug.LogError("没有找到绑定信息 或者 没有绑定信息 请检查");
                return false;
            }

            g_UITypeToPkgInfo = new Dictionary<Type, YIUIBindVo>(binds.Length);
            g_UIPathToPkgInfo = new Dictionary<string, Dictionary<string, YIUIBindVo>>(binds.Length);
            g_UIToPkgInfo     = new Dictionary<string, YIUIBindVo>(binds.Length);

            for (var i = 0; i < binds.Length; i++)
            {
                var vo = binds[i];
                g_UITypeToPkgInfo[vo.ComponentType] = vo;
                AddPkgInfoToPathDic(vo);
                g_UIToPkgInfo[vo.ResName] = vo;
            }

            IsInit = true;
            return true;
        }

        private static void AddPkgInfoToPathDic(YIUIBindVo vo)
        {
            var pkgName = vo.PkgName;
            var resName = vo.ResName;
            if (!g_UIPathToPkgInfo.ContainsKey(pkgName))
            {
                g_UIPathToPkgInfo.Add(pkgName, new Dictionary<string, YIUIBindVo>());
            }

            var dic = g_UIPathToPkgInfo[pkgName];

            if (dic.ContainsKey(resName))
            {
                Debug.LogError($"重复资源 请检查 {pkgName} {resName}");
                return;
            }

            dic.Add(resName, vo);
        }

        /// <summary>
        /// 得到UI包信息
        /// </summary>
        /// <param name="uiType"></param>
        /// <returns></returns>
        public static YIUIBindVo? GetBindVoByType(Type uiType)
        {
            if (uiType == null)
            {
                Debug.LogError($"空 无法取到这个包信息 请检查");
                return null;
            }

            if (g_UITypeToPkgInfo.TryGetValue(uiType, out var vo))
            {
                return vo;
            }

            Debug.LogError($"未获取到这个UI包信息 请检查  {uiType.Name}");
            return null;
        }

        /// <summary>
        /// 得到UI包信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static YIUIBindVo? GetBindVoByType<T>()
        {
            return GetBindVoByType(typeof (T));
        }

        /// <summary>
        /// 根据唯一ID获取
        /// 由pkg+res 拼接的唯一ID
        /// </summary>
        public static YIUIBindVo? GetBindVoByPath(string pkgName, string resName)
        {
            if (string.IsNullOrEmpty(pkgName) || string.IsNullOrEmpty(resName))
            {
                Debug.LogError($"空名称 无法取到这个包信息 请检查");
                return null;
            }

            if (!g_UIPathToPkgInfo.ContainsKey(pkgName))
            {
                Debug.LogError($"不存在这个包信息 请检查 {pkgName}");
                return null;
            }

            if (g_UIPathToPkgInfo[pkgName].TryGetValue(resName, out var vo))
            {
                return vo;
            }

            Debug.LogError($"未获取到这个包信息 请检查  {pkgName} {resName}");

            return null;
        }

        /// <summary>
        /// 根据resName获取
        /// 只有保证所有res唯一时才可使用
        /// </summary>
        internal static YIUIBindVo? GetBindVoByResName(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                Debug.LogError($"空名称 无法取到这个包信息 请检查");
                return null;
            }

            if (g_UIToPkgInfo.TryGetValue(resName, out var vo))
            {
                return vo;
            }

            Debug.LogError($"未获取到这个包信息 请检查  {resName}");

            return null;
        }

        /// <summary>
        /// 重置 慎用
        /// </summary>
        internal static void Reset()
        {
            if (g_UITypeToPkgInfo != null)
            {
                g_UITypeToPkgInfo.Clear();
                g_UITypeToPkgInfo = null;
            }

            if (g_UIPathToPkgInfo != null)
            {
                g_UIPathToPkgInfo.Clear();
                g_UIPathToPkgInfo = null;
            }

            if (g_UIToPkgInfo != null)
            {
                g_UIToPkgInfo.Clear();
                g_UIToPkgInfo = null;
            }

            IsInit = false;
        }
    }
}