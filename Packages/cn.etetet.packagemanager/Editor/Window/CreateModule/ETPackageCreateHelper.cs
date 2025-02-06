using System;
using System.IO;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public static partial class ETPackageCreateHelper
    {
        private const string Pattern = "[^a-z]";

        public static string ETPackageCreateTemplatePath = "Packages/cn.etetet.packagemanager/Editor/Window/CreateModule/Template";

        public static string GetPackagePath(ref string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError($"包名不能为空");
                return "";
            }

            packageName = packageName.ToLower();
            packageName = System.Text.RegularExpressions.Regex.Replace(packageName, Pattern, "");
            var packagepath = $"Packages/cn.etetet.{packageName}";

            return packagepath;
        }

        public static bool CreateDirectory(string path, bool force = true)
        {
            if (Directory.Exists(path))
            {
                if (force)
                {
                    Directory.Delete(path, true);
                }
                else
                {
                    return false;
                }
            }

            Directory.CreateDirectory(path);
            return true;
        }

        public static void CreatePackage(ETPackageCreateData data)
        {
            var createTypeValues = Enum.GetValues(typeof(EPackageCreateType));

            foreach (var value in createTypeValues)
            {
                var createType = (EPackageCreateType)value;
                if ((int)createType <= 0)
                {
                    continue;
                }

                var hasReslut = data.PackageCreateType.HasFlag(createType);
                if (hasReslut)
                {
                    switch (createType)
                    {
                        case EPackageCreateType.Runtime:
                            new ETPackageCreatePackageAsmdefCode(data);
                            break;
                        case EPackageCreateType.Editor:
                            new ETPackageCreatePackageAsmdefEditorCode(data);
                            break;
                        case EPackageCreateType.Hotfix:
                            var hotfixClient = data.FolderType.HasFlag(EPackageCreateFolderType.Client);
                            if (hotfixClient)
                            {
                                CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/Hotfix/Client"));
                            }

                            var hotfixServer = data.FolderType.HasFlag(EPackageCreateFolderType.Server);
                            if (hotfixServer)
                            {
                                CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/Hotfix/Server"));
                            }

                            var hotfixShare = data.FolderType.HasFlag(EPackageCreateFolderType.Share);
                            if (hotfixShare)
                            {
                                CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/Hotfix/Share"));
                            }

                            break;
                        case EPackageCreateType.HotfixView:
                            CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/HotfixView/Client"));
                            break;
                        case EPackageCreateType.Model:
                            var modelClient = data.FolderType.HasFlag(EPackageCreateFolderType.Client);
                            if (modelClient)
                            {
                                CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/Model/Client"));
                            }

                            var modelServer = data.FolderType.HasFlag(EPackageCreateFolderType.Server);
                            if (modelServer)
                            {
                                CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/Model/Server"));
                            }

                            var modelShare = data.FolderType.HasFlag(EPackageCreateFolderType.Share);
                            if (modelShare)
                            {
                                CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/Model/Share"));
                            }

                            break;
                        case EPackageCreateType.ModelView:
                            CreateDirectory(EditorHelper.GetProjPath($"{data.PackagePath}/Scripts/ModelView/Client"));
                            break;
                        default:
                            Debug.LogError($"未实现的功能 {createType}");
                            break;
                    }
                }
            }

            new ETPackageCreatePackageIgnoreAsmdefCode(data);
            new ETPackageCreatePackageJsonCode(data);
            new ETPackageCreatePackageGitCode(data);
        }
    }
}
