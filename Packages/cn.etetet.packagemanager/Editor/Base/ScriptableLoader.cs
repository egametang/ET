using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public static class ScriptableLoader
    {
        public static T Load<T>() where T : ScriptableObject
        {
            var settingType = typeof(T);
            var guids       = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogError($"没有这个文件 : {settingType.Name}");
                return null;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogError($"找到多个文件 : {path}");
                    }
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var    asset    = AssetDatabase.LoadAssetAtPath<T>(filePath);
                return asset;
            }
        }
    }
}