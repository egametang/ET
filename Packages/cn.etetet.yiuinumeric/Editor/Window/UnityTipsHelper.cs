using System;
using UnityEditor;
using UnityEngine;

namespace YIUI.Numeric.Editor
{
    public static class UnityTipsHelper
    {
        public static void CallBack(string content, Action okCallBack, Action cancelCallBack = null)
        {
            var selectIndex = EditorUtility.DisplayDialogComplex("提示", content, "确认", "取消", null);
            if (selectIndex == 0)
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
        }

        public static void Show(string content)
        {
            EditorUtility.DisplayDialog("提示", content, "确认");
        }

        public static void ShowError(string message)
        {
            Show(message);
            Debug.LogError(message);
        }

        public static void SelectLubanFolder(string packageName)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"Packages/{packageName}/Assets/Editor/Luban");
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }
    }
}