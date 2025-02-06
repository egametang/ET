using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public class AssetBundleCollectorSettingData
    {
        private static readonly Dictionary<string, System.Type> _cacheActiveRuleTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, IActiveRule> _cacheActiveRuleInstance = new Dictionary<string, IActiveRule>();

        private static readonly Dictionary<string, System.Type> _cacheAddressRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IAddressRule> _cacheAddressRuleInstance = new Dictionary<string, IAddressRule>();

        private static readonly Dictionary<string, System.Type> _cachePackRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IPackRule> _cachePackRuleInstance = new Dictionary<string, IPackRule>();

        private static readonly Dictionary<string, System.Type> _cacheFilterRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IFilterRule> _cacheFilterRuleInstance = new Dictionary<string, IFilterRule>();

        /// <summary>
        /// 配置数据是否被修改
        /// </summary>
        public static bool IsDirty { private set; get; } = false;


        static AssetBundleCollectorSettingData()
        {
            // IPackRule
            {
                // 清空缓存集合
                _cachePackRuleTypes.Clear();
                _cachePackRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(PackSeparately),
                    typeof(PackDirectory),
                    typeof(PackTopDirectory),
                    typeof(PackCollector),
                    typeof(PackGroup),
                    typeof(PackRawFile),
                    typeof(PackShaderVariants)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IPackRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (_cachePackRuleTypes.ContainsKey(type.Name) == false)
                        _cachePackRuleTypes.Add(type.Name, type);
                }
            }

            // IFilterRule
            {
                // 清空缓存集合
                _cacheFilterRuleTypes.Clear();
                _cacheFilterRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(CollectAll),
                    typeof(CollectScene),
                    typeof(CollectPrefab),
                    typeof(CollectSprite)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IFilterRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (_cacheFilterRuleTypes.ContainsKey(type.Name) == false)
                        _cacheFilterRuleTypes.Add(type.Name, type);
                }
            }

            // IAddressRule
            {
                // 清空缓存集合
                _cacheAddressRuleTypes.Clear();
                _cacheAddressRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(AddressByFileName),
                    typeof(AddressByFolderAndFileName),
                    typeof(AddressByGroupAndFileName),
                    typeof(AddressDisable)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IAddressRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (_cacheAddressRuleTypes.ContainsKey(type.Name) == false)
                        _cacheAddressRuleTypes.Add(type.Name, type);
                }
            }

            // IActiveRule
            {
                // 清空缓存集合
                _cacheActiveRuleTypes.Clear();
                _cacheActiveRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(EnableGroup),
                    typeof(DisableGroup),
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IActiveRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (_cacheActiveRuleTypes.ContainsKey(type.Name) == false)
                        _cacheActiveRuleTypes.Add(type.Name, type);
                }
            }
        }

        private static AssetBundleCollectorSetting _setting = null;
        public static AssetBundleCollectorSetting Setting
        {
            get
            {
                if (_setting == null)
                    _setting = SettingLoader.LoadSettingData<AssetBundleCollectorSetting>();
                return _setting;
            }
        }

        /// <summary>
        /// 存储配置文件
        /// </summary>
        public static void SaveFile()
        {
            if (Setting != null)
            {
                IsDirty = false;
                EditorUtility.SetDirty(Setting);
                AssetDatabase.SaveAssets();
                Debug.Log($"{nameof(AssetBundleCollectorSetting)}.asset is saved!");
            }
        }

        /// <summary>
        /// 修复配置文件
        /// </summary>
        public static void FixFile()
        {
            bool isFixed = Setting.FixAllPackageConfigError();
            if (isFixed)
            {
                IsDirty = true;
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public static void ClearAll()
        {
            Setting.ClearAll();
            SaveFile();
        }

        public static List<RuleDisplayName> GetActiveRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in _cacheActiveRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }
        public static List<RuleDisplayName> GetAddressRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in _cacheAddressRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }
        public static List<RuleDisplayName> GetPackRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in _cachePackRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }
        public static List<RuleDisplayName> GetFilterRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in _cacheFilterRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }
        private static string GetRuleDisplayName(string name, Type type)
        {
            var attribute = DisplayNameAttributeHelper.GetAttribute<DisplayNameAttribute>(type);
            if (attribute != null && string.IsNullOrEmpty(attribute.DisplayName) == false)
                return attribute.DisplayName;
            else
                return name;
        }

        public static bool HasActiveRuleName(string ruleName)
        {
            return _cacheActiveRuleTypes.Keys.Contains(ruleName);
        }
        public static bool HasAddressRuleName(string ruleName)
        {
            return _cacheAddressRuleTypes.Keys.Contains(ruleName);
        }
        public static bool HasPackRuleName(string ruleName)
        {
            return _cachePackRuleTypes.Keys.Contains(ruleName);
        }
        public static bool HasFilterRuleName(string ruleName)
        {
            return _cacheFilterRuleTypes.Keys.Contains(ruleName);
        }

        public static IActiveRule GetActiveRuleInstance(string ruleName)
        {
            if (_cacheActiveRuleInstance.TryGetValue(ruleName, out IActiveRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheActiveRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IActiveRule)Activator.CreateInstance(type);
                _cacheActiveRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(IActiveRule)} is invalid：{ruleName}");
            }
        }
        public static IAddressRule GetAddressRuleInstance(string ruleName)
        {
            if (_cacheAddressRuleInstance.TryGetValue(ruleName, out IAddressRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheAddressRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IAddressRule)Activator.CreateInstance(type);
                _cacheAddressRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(IAddressRule)} is invalid：{ruleName}");
            }
        }
        public static IPackRule GetPackRuleInstance(string ruleName)
        {
            if (_cachePackRuleInstance.TryGetValue(ruleName, out IPackRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cachePackRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IPackRule)Activator.CreateInstance(type);
                _cachePackRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(IPackRule)} is invalid：{ruleName}");
            }
        }
        public static IFilterRule GetFilterRuleInstance(string ruleName)
        {
            if (_cacheFilterRuleInstance.TryGetValue(ruleName, out IFilterRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheFilterRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IFilterRule)Activator.CreateInstance(type);
                _cacheFilterRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(IFilterRule)} is invalid：{ruleName}");
            }
        }

        // 公共参数编辑相关
        public static void ModifyShowPackageView(bool showPackageView)
        {
            Setting.ShowPackageView = showPackageView;
            IsDirty = true;
        }
        public static void ModifyShowEditorAlias(bool showAlias)
        {
            Setting.ShowEditorAlias = showAlias;
            IsDirty = true;
        }
        public static void ModifyUniqueBundleName(bool uniqueBundleName)
        {
            Setting.UniqueBundleName = uniqueBundleName;
            IsDirty = true;
        }

        // 资源包裹编辑相关
        public static AssetBundleCollectorPackage CreatePackage(string packageName)
        {
            AssetBundleCollectorPackage package = new AssetBundleCollectorPackage();
            package.PackageName = packageName;
            Setting.Packages.Add(package);
            IsDirty = true;
            return package;
        }
        public static void RemovePackage(AssetBundleCollectorPackage package)
        {
            if (Setting.Packages.Remove(package))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed remove package : {package.PackageName}");
            }
        }
        public static void ModifyPackage(AssetBundleCollectorPackage package)
        {
            if (package != null)
            {
                IsDirty = true;
            }
        }

        // 资源分组编辑相关
        public static AssetBundleCollectorGroup CreateGroup(AssetBundleCollectorPackage package, string groupName)
        {
            AssetBundleCollectorGroup group = new AssetBundleCollectorGroup();
            group.GroupName = groupName;
            package.Groups.Add(group);
            IsDirty = true;
            return group;
        }
        public static void RemoveGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
        {
            if (package.Groups.Remove(group))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed remove group : {group.GroupName}");
            }
        }
        public static void ModifyGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
        {
            if (package != null && group != null)
            {
                IsDirty = true;
            }
        }

        // 资源收集器编辑相关
        public static void CreateCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            group.Collectors.Add(collector);
            IsDirty = true;
        }
        public static void RemoveCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            if (group.Collectors.Remove(collector))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed remove collector : {collector.CollectPath}");
            }
        }
        public static void ModifyCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            if (group != null && collector != null)
            {
                IsDirty = true;
            }
        }

        /// <summary>
        /// 获取所有的资源标签
        /// </summary>
        public static string GetPackageAllTags(string packageName)
        {
            var allTags = Setting.GetPackageAllTags(packageName);
            return string.Join(";", allTags);
        }
    }
}