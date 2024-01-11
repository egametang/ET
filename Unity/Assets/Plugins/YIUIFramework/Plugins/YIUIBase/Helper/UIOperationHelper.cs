using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YIUIFramework
{
    /// <summary>
    /// 检查是否可以进行UI操作
    /// </summary>
    public static class UIOperationHelper
    {
        public static bool IsPlaying()
        {
            if (Application.isPlaying)
            {
                //编辑时点结束的一瞬间 还是在运行中的 所以要判断是否正在退出
                //如果正在退出 算非运行
                if (SingletonMgr.IsQuitting)
                {
                    return false;
                }
                return true;
            }
            
            #if UNITY_EDITOR
            //编辑器时 点开始的一瞬间是不算正在运行的 在我这里算运行中
            return EditorApplication.isPlayingOrWillChangePlaymode; 
            #endif
            
            return false;
        }

        public static bool RunTimeCheckIsPlaying(bool log = true)
        {
            if (IsPlaying())
            {
                if (log)
                    Debug.LogError($"当前正在运行时 请不要在运行时使用");
                return false;
            }

            return true;
        }

        #if UNITY_EDITOR

        //UI通用判断 运行时是否可显示
        //通过切换宏可以在运行时提供可修改
        public static bool CommonShowIf()
        {
            #if YIUIMACRO_BIND_RUNTIME_EDITOR
            return true;
            #endif

            if (IsPlaying())
            {
                return false;
            }

            return true;
        }

        //运行时不可用
        public static bool CheckUIOperation(bool log = true)
        {
            if (IsPlaying())
            {
                if (log)
                    UnityTipsHelper.ShowError($"当前正在运行时 请不要在运行时使用");
                return false;
            }

            return true;
        }

        public static bool CheckUIOperation(Object obj, bool log = true)
        {
            if (IsPlaying())
            {
                if (log)
                    UnityTipsHelper.ShowErrorContext(obj, $"当前正在运行时 请不要在运行时使用");
                return false;
            }

            var checkInstance = PrefabUtility.IsPartOfPrefabInstance(obj);
            if (checkInstance)
            {
                if (log)
                    UnityTipsHelper.ShowErrorContext(obj, $"不能对实体进行操作  必须进入预制体编辑!!!");
                return false;
            }

            return true;
        }

        public static bool CheckUIOperationAll(Object obj, bool log = true)
        {
            if (IsPlaying())
            {
                if (log)
                    UnityTipsHelper.ShowErrorContext(obj, $"当前正在运行时 请不要在运行时使用");
                return false;
            }

            var checkInstance = PrefabUtility.IsPartOfPrefabInstance(obj);
            if (checkInstance)
            {
                if (log)
                    UnityTipsHelper.ShowErrorContext(obj, $"不能对实体进行操作  必须进入预制体编辑!!!");
                return false;
            }

            var checkAsset = PrefabUtility.IsPartOfPrefabAsset(obj);
            if (!checkAsset)
            {
                if (log)
                    UnityTipsHelper.ShowErrorContext(obj, $"1: 必须是预制体 2: 不能在Hierarchy面板中使用 必须在Project面板下的预制体原件才能使用使用 ");
                return false;
            }

            return true;
        }

        #endif
    }
}