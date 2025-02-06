#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    [Flags]
    public enum EPackagesFilterType
    {
        [LabelText("全部")]
        All = 1 << 30,

        [LabelText("无")]
        None = 1,

        [LabelText("ET包")]
        ET = 1 << 1,

        [LabelText("更新")]
        Update = 1 << 2,

        [LabelText("请求")]
        Req = 1 << 3,

        [LabelText("禁用")]
        Ban = 1 << 4,

        [LabelText("解禁")]
        ReBan = 1 << 5,
    }

    public enum EPackagesFilterOperationType
    {
        [LabelText("唯一")]
        Only = 0,

        [LabelText("或")]
        Or = 1,

        [LabelText("与")]
        And = 2,
    }

    /// <summary>
    /// 版本管理
    /// </summary>
    [ETPackageMenu("版本管理", 1000)]
    public partial class ETPackageVersionModule : BasePackageToolModule
    {
        public static ETPackageVersionModule Inst;

        [HideLabel]
        [HideIf("CheckUpdateAllEnd")]
        [ShowInInspector]
        [DisplayAsString(false, 100, TextAlignment.Center, true)]
        private const string m_CheckUpdateAllReqing = "请求所有包最新版本中...";

        [BoxGroup("信息", centerLabel: true)]
        [EnumToggleButtons]
        [HideLabel]
        [OnValueChanged("OnFilterOperationTypeChanged")]
        [ShowIf("CheckUpdateAllEnd")]
        [PropertyOrder(-666)]
        public EPackagesFilterOperationType FilterOperationType;

        private void OnFilterOperationTypeChanged()
        {
            if (FilterOperationType == EPackagesFilterOperationType.Only)
            {
                LastFilterType = EPackagesFilterType.None;
                FilterType     = EPackagesFilterType.None;
            }

            LoadFilterPackageInfoData();
        }

        [BoxGroup("信息", centerLabel: true)]
        [EnumToggleButtons]
        [HideLabel]
        [OnValueChanged("OnFilterTypeChanged")]
        [ShowIf("CheckUpdateAllEnd")]
        [PropertyOrder(-666)]
        public EPackagesFilterType FilterType;

        private EPackagesFilterType LastFilterType;

        private void OnFilterTypeChanged()
        {
            if (FilterOperationType == EPackagesFilterOperationType.Only)
            {
                if (FilterType != LastFilterType)
                {
                    var current = (int)FilterType;
                    var last    = (int)LastFilterType;
                    FilterType = (EPackagesFilterType)(current > last ? current - last : last - current);
                }
            }

            LastFilterType = FilterType;
            LoadFilterPackageInfoData();
        }

        [BoxGroup("信息", centerLabel: true)]
        [LabelText("搜索 (支持正则)")]
        [ShowIf("CheckUpdateAllEnd")]
        [OnValueChanged("OnSearchChanged")]
        [Delayed]
        [ShowInInspector]
        [PropertyOrder(-444)]
        private string Search = "";

        private void OnSearchChanged()
        {
            Search = Search.ToLower();
            LoadFilterPackageInfoData();
        }

        private EnumPrefs<EPackagesFilterType> FilterTypePrefs = new("ETPackageVersionModule_FilterType", null, EPackagesFilterType.ET);

        private EnumPrefs<EPackagesFilterOperationType> FilterOperationTypePrefs = new("ETPackageVersionModule_FilterOperationType");

        private BoolPrefs   SyncDependencyPrefs = new("ETPackageVersionModule_SyncDependency", null, true);
        private StringPrefs SearchPrefs         = new("ETPackageVersionModule_Search", null, "");

        public bool CheckUpdateAllEnd { get; private set; }

        public bool RequestAllResult { get; private set; }

        public override void Initialize()
        {
            CheckUpdateAllEnd = false;
            RequestAllResult  = false;
            PackageHelper.CheckUpdateAll((result) =>
            {
                CheckUpdateAllEnd = true;
                RequestAllResult  = result;
                if (!result) return;
                Search              = SearchPrefs.Value;
                FilterType          = FilterTypePrefs.Value;
                LastFilterType      = FilterType;
                FilterOperationType = FilterOperationTypePrefs.Value;
                LoadAllPackageInfoData();
                LoadFilterPackageInfoData();
                Inst = this;
            });
        }

        public override void OnDestroy()
        {
            Inst                           = null;
            SearchPrefs.Value              = Search;
            FilterTypePrefs.Value          = FilterType;
            FilterOperationTypePrefs.Value = FilterOperationType;
        }

        [Button("同步生成", 50)]
        [GUIColor(1, 1, 0)]
        [PropertyOrder(-888)]
        [ShowIf("CheckUpdateAllEnd")]
        public void SyncPackages()
        {
            UnityTipsHelper.CallBack($"确定同步生成当前所有改动版本?", () => { UpdatePackagesInfo(); });
        }

        [Button("文档", 30, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        [PropertyOrder(-999)]
        [ShowIf("CheckUpdateAllEnd")]
        public void OpenDocument()
        {
            ETPackageDocumentModule.ETPackageVersion();
        }

        private async Task UpdatePackagesInfo(bool close = true)
        {
            var count = m_AllPackageInfoDataDic.Count;
            var index = 0;
            foreach (var data in m_AllPackageInfoDataDic)
            {
                index++;
                var packageInfo = data.Value;
                if (!CheckPackageChange(packageInfo)) continue;
                EditorUtility.DisplayProgressBar("同步信息", $"更新{packageInfo.Name}", index * 1f / count);
                await ChangePackageInfo(packageInfo);
            }

            ETPackageAutoTool.UnloadAllAssets();

            if (close)
            {
                PackageExecuteMenuItemHelper.ET_Init_RepairDependencies();
                EditorUtility.ClearProgressBar();
                ETPackageAutoTool.CloseWindowRefresh();
            }
        }

        private bool CheckPackageChange(PackageVersionData packageInfoData)
        {
            if (!packageInfoData.IsETPackage)
            {
                return false;
            }

            var oldAllPackage = PackageVersionHelper.PackageVersionAsset.AllPackageVersionData;
            if (!oldAllPackage.ContainsKey(packageInfoData.Name))
            {
                return false;
            }

            var oldPackageInfo = oldAllPackage[packageInfoData.Name];

            //对比版本
            if (packageInfoData.Version != oldPackageInfo.Version)
            {
                return true;
            }

            //对比依赖
            if (packageInfoData.Dependencies.Count != oldPackageInfo.Dependencies.Count)
            {
                return true;
            }

            for (int i = 0; i < packageInfoData.Dependencies.Count; i++)
            {
                var dependency    = packageInfoData.Dependencies[i];
                var oldDependency = oldPackageInfo.Dependencies[i];
                if (dependency.Name != oldDependency.Name || dependency.Version != oldDependency.Version)
                {
                    return true;
                }
            }

            //对比依赖我
            if (packageInfoData.DependenciesSelf.Count != oldPackageInfo.DependenciesSelf.Count)
            {
                return true;
            }

            for (int i = 0; i < packageInfoData.DependenciesSelf.Count; i++)
            {
                var dependency    = packageInfoData.DependenciesSelf[i];
                var oldDependency = oldPackageInfo.DependenciesSelf[i];
                if (dependency.Name != oldDependency.Name || dependency.Version != oldDependency.Version)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ChangePackageInfo(PackageVersionData packageInfoData)
        {
            var assetPath   = $"Packages/{packageInfoData.Name}/package.json";
            var packagePath = $"{Application.dataPath}/../{assetPath}";
            if (!File.Exists(packagePath))
            {
                //Debug.LogError($"包 {packageInfoData.Name} 路径 {packagePath} 不存在");
                return;
            }

            var changeVersion = Regex.Replace(packageInfoData.Version, Pattern, "");

            try
            {
                string fileContent = await File.ReadAllTextAsync(packagePath);

                JObject json = JObject.Parse(fileContent);

                json["version"]      = changeVersion;
                json["dependencies"] = NewDependencies(packageInfoData);

                await File.WriteAllTextAsync(packagePath, json.ToString(), System.Text.Encoding.UTF8);

                Debug.Log($"修改成功 {packageInfoData.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"错误 修改包 {packageInfoData.Name} 失败 {e.Message}");
                return;
            }
        }

        private JObject NewDependencies(PackageVersionData packageInfoData)
        {
            JObject newDependencies = new JObject();
            foreach (var dependency in packageInfoData.Dependencies)
            {
                newDependencies[dependency.Name] = Regex.Replace(dependency.Version, Pattern, "");
            }

            return newDependencies;
        }

        [TableList(DrawScrollView = true, AlwaysExpanded = true, IsReadOnly = true)]
        [NonSerialized]
        [BoxGroup("筛选包数据", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [ShowIf("CheckUpdateAllEnd")]
        [PropertyOrder(999)]
        private List<PackageVersionData> m_FilterPackageInfoDataList = new();

        private readonly Dictionary<string, PackageVersionData> m_FilterPackageInfoDataDic = new();

        private Dictionary<string, PackageVersionData> m_AllPackageInfoDataDic;

        public PackageVersionData GetPackageInfoData(string packageName)
        {
            m_AllPackageInfoDataDic.TryGetValue(packageName, out PackageVersionData packageInfoData);
            return packageInfoData;
        }

        private void LoadFilterPackageInfoData()
        {
            m_FilterPackageInfoDataList.Clear();
            m_FilterPackageInfoDataDic.Clear();
            var packagesFilterTypeValues = Enum.GetValues(typeof(EPackagesFilterType));

            foreach (var data in m_AllPackageInfoDataDic)
            {
                var name = data.Key;

                if (!string.IsNullOrEmpty(Search) && !Regex.IsMatch(name, Search))
                {
                    continue;
                }

                var infoData = data.Value;
                var add      = false;

                switch (FilterOperationType)
                {
                    case EPackagesFilterOperationType.Only:
                        add = GetResult(FilterType);
                        break;
                    case EPackagesFilterOperationType.Or:
                        add = false;
                        foreach (var value in packagesFilterTypeValues)
                        {
                            var filterTypeValue = (EPackagesFilterType)value;
                            var hasReslut       = FilterType.HasFlag(filterTypeValue);
                            if (hasReslut)
                            {
                                add = GetResult(filterTypeValue);
                                if (add)
                                {
                                    break;
                                }
                            }
                        }

                        break;
                    case EPackagesFilterOperationType.And:
                        add = true;
                        foreach (var value in packagesFilterTypeValues)
                        {
                            var filterTypeValue = (EPackagesFilterType)value;
                            var hasReslut       = FilterType.HasFlag(filterTypeValue);
                            if (hasReslut)
                            {
                                add = GetResult(filterTypeValue);
                                if (!add)
                                {
                                    break;
                                }
                            }
                        }

                        break;
                    default:
                        //Debug.LogError($"未实现的操作类型 {FilterOperationType}");
                        break;
                }

                if (!add) continue;

                m_FilterPackageInfoDataList.Add(infoData);
                m_FilterPackageInfoDataDic.Add(name, infoData);

                continue;

                bool GetResult(EPackagesFilterType filterValue)
                {
                    var result = false;
                    switch (filterValue)
                    {
                        case EPackagesFilterType.All:
                            result = true;
                            break;
                        case EPackagesFilterType.None:
                            result = false;
                            break;
                        case EPackagesFilterType.ET:
                            result = infoData.IsETPackage;
                            break;
                        case EPackagesFilterType.Update:
                            result = infoData.CanUpdateVersion;
                            break;
                        case EPackagesFilterType.Req:
                            result = infoData.ShowIfReqVersion();
                            break;
                        case EPackagesFilterType.Ban:
                            result = infoData.ShowIfBanReqVersion();
                            break;
                        case EPackagesFilterType.ReBan:
                            result = infoData.ShowIfReBanReqVersion();
                            break;
                        default:
                            //Debug.LogError($"新增了筛选条件 请扩展 {filterValue}");
                            break;
                    }

                    return result;
                }
            }
        }

        private void LoadAllPackageInfoData()
        {
            m_AllPackageInfoDataDic = (Dictionary<string, PackageVersionData>)SerializationUtility.CreateCopy(PackageVersionHelper.PackageVersionAsset.AllPackageVersionData);

            //处理依赖检查
            foreach (var data in m_AllPackageInfoDataDic.Values)
            {
                var name         = data.Name;
                var dependencies = data.Dependencies;
                foreach (var dependency in dependencies)
                {
                    if (!m_AllPackageInfoDataDic.ContainsKey(dependency.Name))
                    {
                        if (dependency.Name.Contains("cn.etetet."))
                        {
                            Debug.LogError($"{name}依赖包{dependency.Name}不存在");
                        }
                        continue;
                    }

                    var target = m_AllPackageInfoDataDic[dependency.Name];

                    if (target.IsETPackage && !CheckVersion(dependency, target))
                    {
                        Debug.LogError($"[{name}]依赖包[{dependency.Name}] 版本不匹配，依赖版本[{dependency.Version}]，当前版本[{target.Version}]");
                    }
                }
            }
        }

        private readonly Dictionary<string, int[]> m_VersionValueDict = new();

        public const string Pattern = "[^0-9.]";

        private int[] GetVersionValue(string version)
        {
            if (m_VersionValueDict.TryGetValue(version, out int[] value))
            {
                return value;
            }

            var versionSplit = Regex.Replace(version, Pattern, "").Split('.');
            m_VersionValueDict[version] = new int[versionSplit.Length];
            for (int i = 0; i < versionSplit.Length; i++)
            {
                if (!int.TryParse(versionSplit[i], out m_VersionValueDict[version][i]))
                {
                    Debug.LogError($"{version} {i} {versionSplit[i]} 不是数字");
                }
            }

            return m_VersionValueDict[version];
        }

        private bool CheckVersion(DependencyInfo dependencyInfo, PackageVersionData targetData)
        {
            var dependencyVersion  = dependencyInfo.Version;
            var versionValue       = GetVersionValue(dependencyVersion);
            var targetVersionValue = targetData?.VersionValue;
            if (targetVersionValue == null)
            {
                Debug.Log($"{targetData.Name} Version == null");
                return false;
            }

            //长度判断
            if (targetVersionValue.Length != versionValue.Length)
            {
                Debug.Log($"{targetData.Name} Version长度不一致，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            //大版本号判断
            if (targetVersionValue.Length >= 1 && targetVersionValue[0] != versionValue[0])
            {
                Debug.Log($"{targetData.Name} 大版本号不匹配，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            //中版本号判断
            if (targetVersionValue.Length >= 2 && targetVersionValue[1] != versionValue[1])
            {
                Debug.Log($"{targetData.Name} 中版本号不匹配，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            //小版本号判断
            if (targetVersionValue.Length >= 3 && targetVersionValue[2] < versionValue[2])
            {
                Debug.Log($"{targetData.Name} 小版本号不匹配，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            return true;
        }
    }
}
#endif