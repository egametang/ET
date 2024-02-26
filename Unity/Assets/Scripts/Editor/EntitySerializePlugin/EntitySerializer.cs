using System;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomEditor(typeof(GameObject))] // 指定要修改的对象类型为 GameObject（Prefab）
    public class MyPrefabEditor : Editor
    {
        [MenuItem("Assets/Edit Entity")] // 创建一个右键菜单项
        private static void SaveToEntity()
        {
            // 获取当前选中的 Prefab
            UnityEngine.Object[] selectedObjects = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Deep);

            foreach (UnityEngine.Object obj in selectedObjects)
            {
                if (!AssetDatabase.Contains(obj))
                {
                    continue; // 确保只处理 Asset 文件而不是场景物体等其他内容
                }

                string path = AssetDatabase.GetAssetPath(obj); // 获取 Prefab 路径
                Type prefabType = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj).GetType(); // 获取原始源的类型

                // 根据需求进行操作，这里仅打印信息
                Debug.Log($"Selected Prefab: {path}, Type: {prefabType}");
            }
        }
    }
}