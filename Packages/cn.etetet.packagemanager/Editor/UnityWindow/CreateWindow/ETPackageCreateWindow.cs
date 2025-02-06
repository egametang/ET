using System;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public class ETPackageCreateWindow : EditorWindow
    {
        #if !ODIN_INSPECTOR
        [MenuItem("ET/ETPackage 创建")]
        #endif
        private static void OpenWindow()
        {
            var window = GetWindow<ETPackageCreateWindow>();
            if (window != null)
                window.Show();
        }

        public static void CloseWindow()
        {
            GetWindow<ETPackageCreateWindow>()?.Close();
        }

        public EPackageCreateType PackageCreateType = EPackageCreateType.All;

        public EPackageRuntimeRefType RuntimeRefType = EPackageRuntimeRefType.All;

        public EPackageCreateFolderType FolderType = EPackageCreateFolderType.All;

        public string PackageAuthor;

        public string PackageName;

        public string PackageId;

        public string DisplayName;

        public string AssemblyName;

        public string Description;

        public bool ForceCreate;

        private string PackagePath;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("提示:安装Odin 可使用更详细丰富的界面");
            GUILayout.Space(10);
            if (GUILayout.Button("ETPackage 创建文档"))
            {
                Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/O6Ukw7k3SiBHmek8MMwc5HdInev");
            }

            GUILayout.Space(10);
            PackageCreateType = (EPackageCreateType)EditorGUILayout.EnumFlagsField("生成类型", PackageCreateType);
            RuntimeRefType    = (EPackageRuntimeRefType)EditorGUILayout.EnumFlagsField("Runtime引用类型", RuntimeRefType);
            FolderType        = (EPackageCreateFolderType)EditorGUILayout.EnumFlagsField("CodeMode类型", FolderType);
            GUILayout.Space(10);
            PackageAuthor = EditorGUILayout.TextField("作者", PackageAuthor);
            PackageName   = EditorGUILayout.TextField("模块名称 cn.etetet.{0}", PackageName);
            PackageId     = EditorGUILayout.TextField("模块ID", PackageId);
            DisplayName   = EditorGUILayout.TextField("显示名称", DisplayName);
            AssemblyName  = EditorGUILayout.TextField("程序集名称", AssemblyName);
            Description   = EditorGUILayout.TextField("描述", Description);
            ForceCreate   = EditorGUILayout.Toggle("强制创建", ForceCreate);

            GUILayout.Space(10);
            if (GUILayout.Button("创建", GUILayout.Height(50)))
            {
                Create();
            }
        }

        private void Create()
        {
            if ((int)PackageCreateType == 0)
            {
                UnityTipsHelper.Show("必须选择生成类型");
                return;
            }

            if (string.IsNullOrEmpty(PackageAuthor))
            {
                UnityTipsHelper.Show("必须输入 作者名称");
                return;
            }

            if (OnRuntimeRefTypeShowIf() && string.IsNullOrEmpty(AssemblyName))
            {
                UnityTipsHelper.Show("必须输入 程序集名称");
                return;
            }

            if (string.IsNullOrEmpty(DisplayName))
            {
                UnityTipsHelper.Show("必须输入 显示名称");
                return;
            }

            if (string.IsNullOrEmpty(PackageName))
            {
                UnityTipsHelper.Show("必须输入 模块名称");
                return;
            }

            PackagePath = ETPackageCreateHelper.GetPackagePath(ref PackageName);
            var projPath = EditorHelper.GetProjPath(PackagePath);

            if (!ETPackageCreateHelper.CreateDirectory(projPath, ForceCreate))
            {
                UnityTipsHelper.Show($"模块[ {PackageName} ] 已存在 请勿重复创建");
                return;
            }

            ETPackageCreateHelper.CreatePackage(
                new ETPackageCreateData
                {
                    PackageAuthor     = this.PackageAuthor,
                    PackagePath       = this.PackagePath,
                    PackageId         = int.Parse(this.PackageId),
                    PackageName       = this.PackageName,
                    AssemblyName      = this.AssemblyName,
                    DisplayName       = this.DisplayName,
                    Description       = this.Description,
                    PackageCreateType = this.PackageCreateType,
                    RuntimeRefType    = this.RuntimeRefType,
                    FolderType        = this.FolderType,
                });

            UnityTipsHelper.Show($"创建成功 [ {PackageName} ]");
            PackageExecuteMenuItemHelper.ETAll();
            CloseWindow();
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        private bool OnRuntimeRefTypeShowIf()
        {
            return PackageCreateType.HasFlag(EPackageCreateType.Runtime);
        }
    }
}
