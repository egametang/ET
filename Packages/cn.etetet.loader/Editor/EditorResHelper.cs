using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ET
{
    public class EditorResHelper
    {
        public static void SaveAssets(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
