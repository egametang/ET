using System.IO;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    public class GlobalProtoEditor: EditorWindow
    {
        const string path = @"./Assets/Res/Config/GlobalProto.txt";

        private GlobalProto globalProto;

        [MenuItem("Tools/全局配置")]
        public static void ShowWindow()
        {
            GetWindow<GlobalProtoEditor>();
        }

        public void Awake()
        {
            if (File.Exists(path))
            {
                this.globalProto = JsonHelper.FromJson<GlobalProto>(File.ReadAllText(path));
            }
            else
            {
                this.globalProto = new GlobalProto();
            }
        }

        public void OnGUI()
        {
            this.globalProto.AssetBundleServerUrl = EditorGUILayout.TextField("资源路径:", this.globalProto.AssetBundleServerUrl);
            this.globalProto.Address = EditorGUILayout.TextField("服务器地址:", this.globalProto.Address);

            if (GUILayout.Button("保存"))
            {
                File.WriteAllText(path, JsonHelper.ToJson(this.globalProto));
                AssetDatabase.Refresh();
            }
        }
    }
}
