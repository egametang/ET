using System;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// Unity提示框@sy
    /// </summary>
    public static class UnityTipsHelper
    {
        /// <summary>
        /// 展示提示
        /// </summary>
        /// <param name="content"></param>
        public static void Show(string content)
        {
            #if UNITY_EDITOR
            EditorUtility.DisplayDialog("提示", content, "确认");
            #endif
        }

        /// <summary>
        /// 提示 同时Log
        /// </summary>
        public static void ShowLog(string message)
        {
            #if UNITY_EDITOR
            Show(message);
            Debug.Log(message);
            #endif
        }

        /// <summary>
        /// 提示 同时Log
        /// </summary>
        public static void ShowLog(object message)
        {
            #if UNITY_EDITOR
            Show(message.ToString());
            Debug.Log(message);
            #endif
        }

        /// <summary>
        /// 提示 同时Warning
        /// </summary>
        public static void ShowWarning(string message)
        {
            #if UNITY_EDITOR
            Show(message);
            Debug.LogWarning(message);
            #endif
        }

        /// <summary>
        /// 提示 同时Warning
        /// </summary>
        public static void ShowWarning(object message)
        {
            #if UNITY_EDITOR
            Show(message.ToString());
            Debug.LogWarning(message);
            #endif
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowError(string message)
        {
            #if UNITY_EDITOR
            Show(message);
            Debug.LogError(message);
            #endif
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowError(object message)
        {
            #if UNITY_EDITOR
            Show(message.ToString());
            Debug.LogError(message);
            #endif
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowErrorContext(Object context, string message)
        {
            #if UNITY_EDITOR
            Show(message);
            Debug.LogError(message, context);
            #endif
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowErrorContext(Object context, object message)
        {
            #if UNITY_EDITOR
            Show(message.ToString());
            Debug.LogError(message, context);
            #endif
        }

        /// <summary>
        /// 确定 取消 回调的提示框
        /// </summary>
        public static void CallBack(string content, Action okCallBack, Action cancelCallBack = null)
        {
            #if UNITY_EDITOR
            var selectIndex = EditorUtility.DisplayDialogComplex("提示", content, "确认", "取消", null);
            if (selectIndex == 0) //确定
            {
                try
                {
                    okCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    cancelCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            #endif
        }

        /// <summary>
        /// 只有确定的提示框
        /// </summary>
        public static void CallBackOk(string content, Action okCallBack, Action cancelCallBack = null)
        {
            #if UNITY_EDITOR
            var result = EditorUtility.DisplayDialog("提示", content, "确认");
            if (result) //确定
            {
                try
                {
                    okCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    cancelCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            #endif
        }
    }
}
