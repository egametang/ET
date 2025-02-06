#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public partial class ETPackageVersionModule
    {
        private bool ShowIfUpdateAll()
        {
            return FilterType == EPackagesFilterType.Update && m_FilterPackageInfoDataList.Count > 0;
        }

        [Button("更新当前所有", 50)]
        [GUIColor(0f, 1f, 0f)]
        [BoxGroup("筛选包数据", centerLabel: true)]
        [ShowIf("ShowIfUpdateAll")]
        [PropertyOrder(99)]
        private void UpdateAll()
        {
            UnityTipsHelper.CallBack($"确定更新所有版本 {m_FilterPackageInfoDataList.Count}个包 \n \n当前更新为覆盖更新模式!!!\n如果需要合并更新请自行解决!!!\n请确保网络没有问题!!!", () =>
            {
                List<string> allName = new();
                foreach (var packageInfo in m_FilterPackageInfoDataList)
                {
                    if (UpdateDependencies(packageInfo))
                    {
                        allName.Add(packageInfo.Name);
                    }
                }

                RequestUpdateAll(allName);
            });
        }

        private bool UpdateDependencies(PackageVersionData packageData)
        {
            var packagePath = Application.dataPath.Replace("Assets", "Packages") + "/" + packageData.Name;

            try
            {
                if (!Directory.Exists(packagePath))
                {
                    return false;
                }

                Directory.Delete(packagePath, true);

                if (Directory.Exists(packagePath))
                {
                    Debug.LogError("删除失败 文件还存在");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"删除文件失败 {e.Message}");
                return false;
            }

            foreach (var data in packageData.DependenciesSelf)
            {
                var packageInfo = GetPackageInfoData(data.Name);
                if (packageInfo == null)
                {
                    return false;
                }

                data.Version = packageData.LastVersion;

                foreach (var info in packageInfo.Dependencies)
                {
                    if (info.Name == packageData.Name)
                    {
                        info.Version = packageData.LastVersion;
                        break;
                    }
                }
            }

            return true;
        }

        private void RequestUpdateAll(List<string> allName)
        {
            if (allName == null || allName.Count == 0)
            {
                Debug.LogError("没有需要更新的包");
                return;
            }

            ETPackageAutoTool.CloseWindow();
            EditorUtility.DisplayProgressBar("同步信息", $"更新中 不要动...动了不负责!! 网络不好可能要等很久...!!", 0);
            new PackageRequestAddAndRemove(allName, (infos) =>
            {
                ETPackageAutoTool.UnloadAllAssets();
                EditorUtility.ClearProgressBar();
                PackageExecuteMenuItemHelper.ETAll();
                AssetDatabase.SaveAssets();
                EditorApplication.ExecuteMenuItem("Assets/Refresh");
            });
        }
    }
}
#endif