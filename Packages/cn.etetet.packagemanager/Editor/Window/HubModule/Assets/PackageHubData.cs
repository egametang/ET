#if ODIN_INSPECTOR
using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class PackageHubData
    {
        [TableColumnWidth(250, Resizable = false)]
        [VerticalGroup("信息")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [LabelWidth(70)]
        [LabelText("包名")]
        public string PackageName;

        [HideInInspector]
        public PackagePayInfo PayInfo;

        public bool PayPackage => PayInfo != null;

        [TableColumnWidth(250, Resizable = false)]
        [VerticalGroup("信息")]
        [ShowInInspector]
        [ReadOnly]
        [LabelWidth(70)]
        [LabelText("售价")]
        [ShowIf("PayPackage")]
        public string PayPackagePrice => PayInfo?.Price ?? "";

        [HideLabel]
        [TableColumnWidth(30, Resizable = false)]
        [VerticalGroup("连接", -999)]
        [Button(30, Icon = SdfIconType.CartFill, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf("PayPackage")]
        public void OpenPackagePay()
        {
            if (string.IsNullOrEmpty(PayInfo.Url))
            {
                PayPackageTips();
            }
            else
            {
                Application.OpenURL(PayInfo.Url);
            }
        }

        [HideLabel]
        [TableColumnWidth(60, Resizable = false)]
        [VerticalGroup("操作")]
        [Button(40, Icon = SdfIconType.CartPlusFill, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf("PayPackage")]
        private void PackagePay()
        {
            PayPackageTips();
        }

        private void PayPackageTips()
        {
            UnityTipsHelper.Show("请ET群联系群主熊猫\nQQ: 80081771\nET群: 474643097\nET新手群: 688514974");
        }

        [TableColumnWidth(250, Resizable = false)]
        [VerticalGroup("信息")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [LabelWidth(70)]
        [LabelText("作者")]
        [HideIf("PayPackage")]
        public string PackageAuthor;

        [NonSerialized]
        [OdinSerialize]
        [HideInInspector]
        public string PackageAuthorURL;

        [HideLabel]
        [TableColumnWidth(30, Resizable = false)]
        [VerticalGroup("连接", -999)]
        [Button(30, Icon = SdfIconType.PersonFill, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf("ShowIfAuthorURL")]
        public void OpenPackageAuthorURL()
        {
            if (string.IsNullOrEmpty(PackageAuthorURL)) return;
            Application.OpenURL(PackageAuthorURL);
        }

        private bool ShowIfAuthorURL()
        {
            return !string.IsNullOrEmpty(PackageAuthorURL);
        }

        [NonSerialized]
        [OdinSerialize]
        [HideInInspector]
        public string PackageRepositoryURL;

        [HideLabel]
        [TableColumnWidth(30, Resizable = false)]
        [VerticalGroup("连接", -999)]
        [Button(30, Icon = SdfIconType.Github, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf("ShowIfRepositoryURL")]
        public void OpenPackageRepositoryURL()
        {
            if (string.IsNullOrEmpty(PackageRepositoryURL)) return;
            Application.OpenURL(PackageRepositoryURL);
        }

        private bool ShowIfRepositoryURL()
        {
            return !string.IsNullOrEmpty(PackageRepositoryURL);
        }

        [TableColumnWidth(250, Resizable = false)]
        [VerticalGroup("信息")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [LabelWidth(70)]
        [LabelText("累计下载")]
        [HideIf("PayPackage")]
        public long DownloadValue;

        [TextArea]
        [VerticalGroup("描述")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [HideLabel]
        public string PackageDescription;

        [LabelWidth(70)]
        [TableColumnWidth(150, Resizable = false)]
        [VerticalGroup("版本")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [LabelText("当前版本")]
        public string PackageCurrentVersion;

        [LabelWidth(70)]
        [TableColumnWidth(150, Resizable = false)]
        [VerticalGroup("版本")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [LabelText("最新版本")]
        public string PackageLastVersion;

        [LabelWidth(70)]
        [TableColumnWidth(150, Resizable = false)]
        [VerticalGroup("版本")]
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [LabelText("类别")]
        public string PackageCategory;

        [HideLabel]
        [TableColumnWidth(60, Resizable = false)]
        [VerticalGroup("操作")]
        [Button(40, Icon = SdfIconType.ArrowRepeat, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf("ShowIfUpdateData")]
        private void UpdateData()
        {
            if (PayPackage) return;
            OperationState = true;
            new PackageRequestTarget(PackageName, (info) =>
            {
                OperationState = false;
                RefreshInfo(info);
            });
        }

        private bool ShowIfUpdateData()
        {
            return !PayPackage && !OperationState;
        }

        [HideLabel]
        [GUIColor("GetRemoveColor")]
        [TableColumnWidth(60, Resizable = false)]
        [VerticalGroup("操作")]
        [Button(40, Icon = SdfIconType.X, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf("ShowIfRemovePackage")]
        private void RemovePackage()
        {
            UnityTipsHelper.CallBackOk($"确定移除 {PackageName}\n \n移除并不包含其他依赖项!!!\n所以如果有其他依赖此包的功能可能会报错!!!\n请确保网络没有问题!!!", () =>
            {
                EditorUtility.DisplayProgressBar("同步信息", $"移除 {PackageName}... 不要动...动了不负责!!", 0);
                OperationState = true;
                new PackageRequestRemove(PackageName, (result) =>
                {
                    ETPackageAutoTool.UnloadAllAssets();
                    OperationState = false;
                    EditorUtility.ClearProgressBar();
                    if (!result) return;
                    m_Install = false;
                    PackageExecuteMenuItemHelper.ETAll();
                    ETPackageAutoTool.CloseWindowRefresh();
                });
            });
        }

        private bool ShowIfRemovePackage()
        {
            return Install && !OperationState && !PayPackage;
        }

        [NonSerialized]
        [OdinSerialize]
        [HideInInspector]
        private bool m_CanRemove;

        private Color GetRemoveColor()
        {
            return m_CanRemove ? Color.HSVToRGB(0f, 0.5f, 1) : Color.HSVToRGB(0f, 0f, 0.5f);
        }

        [HideLabel]
        [TableColumnWidth(60, Resizable = false)]
        [VerticalGroup("操作")]
        [Button(40, Icon = SdfIconType.ArrowDownShort, IconAlignment = IconAlignment.LeftOfText)]
        [GUIColor(0.4f, 0.8f, 1)]
        [HideIf("HideIfInstallPackage")]
        private void InstallPackage()
        {
            UnityTipsHelper.CallBackOk($"确定安装 {PackageName}\n \n请保证网络流程!!", () =>
            {
                EditorUtility.DisplayProgressBar("同步信息", $"安装{PackageName}中 不要动...动了不负责!! 网络不好可能要等很久...!!", 0);
                OperationState = true;
                new PackageRequestAdd(PackageName, (info) =>
                {
                    if (info != null)
                    {
                        m_Install             = true;
                        PackageCurrentVersion = info.version;
                    }

                    OperationState = false;
                    ETPackageAutoTool.UnloadAllAssets();
                    EditorUtility.ClearProgressBar();
                    PackageExecuteMenuItemHelper.ETAll();
                    ETPackageAutoTool.CloseWindowRefresh();
                });
            });
        }

        private bool HideIfInstallPackage()
        {
            return Install || OperationState || PayPackage;
        }

        [NonSerialized]
        [OdinSerialize]
        [HideInInspector]
        private bool m_Install;

        [HideInInspector]
        public bool Install => m_Install;

        public bool OperationState { get; set; }

        [VerticalGroup("操作")]
        [HideLabel]
        [ShowIf("OperationState")]
        [ShowInInspector]
        [TextArea]
        [DisplayAsString(false, 15, TextAlignment.Center, true)]
        private const string m_CheckUpdateAllReqing = "操作中";

        public void RefreshInfo(UnityEditor.PackageManager.PackageInfo info)
        {
            if (PayPackage) return;
            if (info == null) return;
            PackageAuthor    = info.author?.name ?? "";
            PackageAuthorURL = info.author?.url ?? "";
            if (string.IsNullOrEmpty(PackageAuthorURL))
            {
                PackageAuthorURL = $"https://github.com/egametang/ET";
            }

            if (info.repository != null)
            {
                var url = info.repository.url ?? "";
                if (string.IsNullOrEmpty(url))
                {
                    PackageRepositoryURL = "";
                }
                else
                {
                    var http      = "http";
                    var httpIndex = url.IndexOf(http, StringComparison.Ordinal);
                    if (httpIndex > 0)
                    {
                        PackageRepositoryURL = url.Substring(httpIndex);
                    }
                    else
                    {
                        PackageRepositoryURL = "";
                    }
                }
            }
            else
            {
                PackageRepositoryURL = "";
            }

            if (string.IsNullOrEmpty(PackageRepositoryURL))
            {
                PackageRepositoryURL = $"https://github.com/ET-Packages/{PackageName}";
            }

            PackageDescription = info.description;
            PackageLastVersion = info.version;
            var currentVersion = PackageHelper.GetPackageCurrentVersion(PackageName);
            m_Install             = !string.IsNullOrEmpty(currentVersion);
            PackageCurrentVersion = m_Install ? currentVersion : "未安装";
            m_CanRemove           = PackageHubHelper.CheckRemove(PackageName);

            if (string.IsNullOrEmpty(info.category))
            {
                var currentInfo = PackageHelper.GetPackageInfo(PackageName);
                PackageCategory = currentInfo == null ? "" : currentInfo.category;
            }
            else
            {
                PackageCategory = info.category;
            }
        }

        public void InitRequestInfo()
        {
            if (string.IsNullOrEmpty(PackageAuthor))
            {
                UpdateData();
            }
        }
    }

    [Serializable]
    [HideReferenceObjectPicker]
    public class PackagePayInfo
    {
        public string Id;
        public string Name;
        public string Description;
        public string Price;
        public string Url;
    }
}
#endif