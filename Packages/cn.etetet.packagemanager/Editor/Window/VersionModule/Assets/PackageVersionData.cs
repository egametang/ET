#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class PackageVersionData
    {
        [ReadOnly]
        [HideLabel]
        [VerticalGroup("信息")]
        [ShowInInspector]
        [PropertyOrder(-50)]
        [OdinSerialize]
        public string Name { get; private set; }

        [ReadOnly]
        [HideLabel]
        [OnValueChanged("OnVersionChanged")]
        [VerticalGroup("信息")]
        [PropertyOrder(-45)]
        [OdinSerialize]
        private string version;

        public string Version
        {
            get => version;
            set
            {
                version     = Regex.Replace(value, ETPackageVersionModule.Pattern, "");
                VersionLong = PackageHelper.GetVersionToLong(version);
                SetVersionValue();
            }
        }

        [HideInInspector]
        public long VersionLong;

        private void OnVersionChanged()
        {
            Version = version;
        }

        [OdinSerialize]
        [HideInInspector]
        public bool IsETPackage { get; set; }

        [OdinSerialize]
        [HideInInspector]
        private int[] m_VersionValue;

        public int[] VersionValue
        {
            get
            {
                return m_VersionValue;
            }
        }

        private void SetVersionValue()
        {
            var versionSplit = Regex.Replace(Version, ETPackageVersionModule.Pattern, "").Split('.');
            m_VersionValue = new int[versionSplit.Length];
            for (int i = 0; i < versionSplit.Length; i++)
            {
                if (!int.TryParse(versionSplit[i], out m_VersionValue[i]))
                {
                    Debug.LogError($"{versionSplit[i]}不是数字");
                }
            }
        }

        [HideReferenceObjectPicker]
        [OdinSerialize]
        [LabelText(" ")]
        [VerticalGroup("我依赖")]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<DependencyInfo> Dependencies = new();

        [HideReferenceObjectPicker]
        [OdinSerialize]
        [LabelText(" ")]
        [VerticalGroup("依赖我")]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<DependencyInfo> DependenciesSelf = new();

        [Button("↓大")]
        [GUIColor(1f, 0.5f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void UpdateVersion()
        {
            if (VersionValue.Length < 3) return;
            VersionValue[0]--;
            if (VersionValue[0] < 0)
            {
                VersionValue[0] = 0;
            }

            Version = string.Join(".", VersionValue);
        }

        [Button("↑大")]
        [GUIColor(1f, 0.65f, 0f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void UpdateVersion2()
        {
            if (VersionValue.Length < 3) return;
            VersionValue[0]++;
            Version = string.Join(".", VersionValue);
        }

        [Button("↓中")]
        [GUIColor(1f, 0.5f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void UpdateVersion3()
        {
            if (VersionValue.Length < 2) return;
            VersionValue[1]--;
            if (VersionValue[1] < 0)
            {
                VersionValue[1] = 0;
            }

            Version = string.Join(".", VersionValue);
        }

        [Button("↑中")]
        [GUIColor(1f, 1f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void UpdateVersion4()
        {
            if (VersionValue.Length < 2) return;
            VersionValue[1]++;
            Version = string.Join(".", VersionValue);
        }

        [Button("↓小")]
        [GUIColor(1f, 0.5f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void UpdateVersion5()
        {
            VersionValue[^1]--;
            if (VersionValue[^1] < 0)
            {
                VersionValue[^1] = 0;
            }

            Version = string.Join(".", VersionValue);
        }

        [Button("↑小")]
        [GUIColor(0.5f, 1f, 0.7f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void UpdateVersion6()
        {
            VersionValue[^1]++;
            Version = string.Join(".", VersionValue);
        }

        [Button("重置")]
        [VerticalGroup("信息")]
        public void ResetVersion()
        {
            var packageInfo = PackageVersionHelper.GetPackageVersionData(Name);
            if (packageInfo == null)
            {
                return;
            }

            Version = packageInfo.Version;
        }

        [Button("请求")]
        [GUIColor(0f, 1f, 1f)]
        [VerticalGroup("信息")]
        [ShowIf("ShowIfReqVersion")]
        [ButtonGroup("信息/请求")]
        public void ReqVersion()
        {
            ReqCheckUpdate();
        }

        public bool ShowIfReqVersion()
        {
            return !IsBan && IsETPackage && string.IsNullOrEmpty(LastVersion);
        }

        [Button("禁")]
        [GUIColor(1f, 0.3f, 0.3f)]
        [VerticalGroup("信息")]
        [ShowIf("ShowIfBanReqVersion")]
        [ButtonGroup("信息/请求")]
        public void BanReqVersion()
        {
            PackageHelper.BanPackage(Name);
            IsBan = true;
        }

        public bool ShowIfBanReqVersion()
        {
            return !IsBan && IsETPackage && string.IsNullOrEmpty(LastVersion);
        }

        [Button("解")]
        [GUIColor(0.3f, 1f, 0.3f)]
        [VerticalGroup("信息")]
        [ShowIf("ShowIfReBanReqVersion")]
        [ButtonGroup("信息/请求")]
        public void ReBanReqVersion()
        {
            PackageHelper.ReBanPackage(Name);
            IsBan = false;
        }

        public bool ShowIfReBanReqVersion()
        {
            return IsBan && IsETPackage && string.IsNullOrEmpty(LastVersion);
        }

        [GUIColor(0f, 1f, 0f)]
        [Button("有最新版本可更新", 50)]
        [VerticalGroup("信息")]
        [ShowIf("CanUpdateVersion")]
        public void CheckUpdateVersion()
        {
            UnityTipsHelper.CallBack($"{Name} 确定更新版本 {Version} >> {LastVersion}\n \n当前更新为覆盖更新模式!!!\n如果需要合并更新请自行解决!!!\n请确保网络没有问题!!!", UpdateDependencies);
        }

        private void UpdateDependencies()
        {
            var packagePath = Application.dataPath.Replace("Assets", "Packages") + "/" + Name;

            try
            {
                if (!System.IO.Directory.Exists(packagePath))
                {
                    return;
                }

                System.IO.Directory.Delete(packagePath, true);

                if (System.IO.Directory.Exists(packagePath))
                {
                    Debug.LogError("删除失败 文件还存在");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"删除文件失败 {e.Message}");
                return;
            }

            foreach (var data in DependenciesSelf)
            {
                var packageInfo = ETPackageVersionModule.Inst.GetPackageInfoData(data.Name);
                if (packageInfo == null)
                {
                    return;
                }

                data.Version = LastVersion;

                foreach (var info in packageInfo.Dependencies)
                {
                    if (info.Name == Name)
                    {
                        info.Version = LastVersion;
                        break;
                    }
                }
            }

            ETPackageAutoTool.CloseWindow();
            EditorUtility.DisplayProgressBar("同步信息", $"更新{Name}中 不要动...动了不负责!! 网络不好可能要等很久...!!", 0);
            new PackageRequestAdd(Name, (info) =>
            {
                ETPackageAutoTool.UnloadAllAssets();
                EditorUtility.ClearProgressBar();
                PackageExecuteMenuItemHelper.ETAll();
                AssetDatabase.SaveAssets();
                EditorApplication.ExecuteMenuItem("Assets/Refresh");
            });
        }

        [HideInInspector]
        [OdinSerialize]
        public bool CanUpdateVersion { get; private set; }

        [HideInInspector]
        [OdinSerialize]
        public bool IsBan { get; private set; }

        [ReadOnly]
        [LabelText("最新版本")]
        [DisplayAsString(false, 20, TextAlignment.Left, true)]
        [VerticalGroup("信息")]
        [ShowInInspector]
        [PropertyOrder(-100)]
        private string DisplayLastVersion
        {
            get
            {
                return $"<color=#00FF00>{LastVersion}</color>";
            }
        }

        [HideInInspector]
        [OdinSerialize]
        private string m_LastVersion;

        public string LastVersion
        {
            get { return m_LastVersion; }
            private set
            {
                m_LastVersion   = value;
                LastVersionLong = PackageHelper.GetVersionToLong(value);
            }
        }

        [HideInInspector]
        public long LastVersionLong;

        public PackageVersionData(string name, string version)
        {
            Name        = name;
            Version     = version;
            IsETPackage = name.Contains("cn.etetet.");
            ReqCheckUpdate();
        }

        private void ReqCheckUpdate()
        {
            if (!IsETPackage) return;

            IsBan = PackageHelper.IsBanPackage(Name);

            if (IsBan) return;

            PackageHelper.CheckUpdateTarget(Name, (lastVersion) =>
            {
                CanUpdateVersion = false;
                if (string.IsNullOrEmpty(lastVersion))
                {
                    return;
                }

                LastVersion = lastVersion;

                if (PackageHelper.GetVersionToLong(lastVersion) > PackageHelper.GetVersionToLong(Version))
                {
                    CanUpdateVersion = true;
                }
            });
        }
    }
}
#endif