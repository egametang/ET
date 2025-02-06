using System.IO;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class AssetBundleInspector
    {
        [CustomEditor(typeof(AssetBundle))]
        internal class AssetBundleEditor : UnityEditor.Editor
        {
            internal bool pathFoldout = false;
            internal bool advancedFoldout = false;
            public override void OnInspectorGUI()
            {
                AssetBundle bundle = target as AssetBundle;

                using (new EditorGUI.DisabledScope(true))
                {
                    var leftStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                    leftStyle.alignment = TextAnchor.UpperLeft;
                    GUILayout.Label(new GUIContent("Name: " + bundle.name), leftStyle);

                    var assetNames = bundle.GetAllAssetNames();
                    pathFoldout = EditorGUILayout.Foldout(pathFoldout, "Source Asset Paths");
                    if (pathFoldout)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var asset in assetNames)
                            EditorGUILayout.LabelField(asset);
                        EditorGUI.indentLevel--;
                    }

                    advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced Data");
                }

                if (advancedFoldout)
                {
                    EditorGUI.indentLevel++;
                    base.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}