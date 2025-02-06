#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public class PackageCategoryModule : BasePackageToolModule
    {
        [Button("文档", 30, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        [PropertyOrder(-9999)]
        public void OpenDocument()
        {
            ETPackageDocumentModule.ETPackageHub();
        }

        [Button("请求刷新所有数据", 50)]
        [PropertyOrder(-999)]
        [GUIColor(0f, 1f, 0f)]
        private void RefreshAll()
        {
            PackageHubHelper.RefreshRequestAll(ShowPackages);
        }

        [TableList(DrawScrollView = true, IsReadOnly = true)]
        [BoxGroup("所有包", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        public List<PackageHubData> ShowPackages = new();

        private PackageCategoryData m_CategoryData;

        private EPackageCategoryType m_CategoryType = EPackageCategoryType.All;

        [BoxGroup("搜索", centerLabel: true)]
        [HideLabel]
        [OnValueChanged("OnSearchChanged")]
        [Delayed]
        [ShowInInspector]
        [PropertyOrder(-99)]
        private string Search = "";

        private List<PackageHubData> AllPackages;

        private void OnSearchChanged()
        {
            if (string.IsNullOrEmpty(Search))
            {
                ShowPackages.Clear();
                ShowPackages.AddRange(AllPackages);
            }
            else
            {
                var search = this.Search.ToLower();
                ShowPackages.Clear();
                foreach (var package in AllPackages)
                {
                    var name   = package.PackageName.ToLower();
                    var author = package.PackageAuthor.ToLower();
                    var desc   = package.PackageDescription.ToLower();
                    if (name.Contains(search) || author.Contains(search) || desc.Contains(search))
                    {
                        ShowPackages.Add(package);
                    }
                }
            }

            ShowPackages.Sort(PackageSort);
        }

        private int PackageSort(PackageHubData x, PackageHubData y)
        {
            //可增加更多的排序
            return x.DownloadValue > y.DownloadValue ? -1 : 1;
        }

        public override void Initialize()
        {
            if (UserData is PackageCategoryData data)
            {
                m_CategoryData = data;
            }
            else
            {
                Debug.LogError("错误的数据: UserData is not PackageCategoryData");
                return;
            }

            AllPackages    = m_CategoryData.AllPackages;
            m_CategoryType = m_CategoryData.CategoryType;
            OnSearchChanged();

            if (m_CategoryType is EPackageCategoryType.All or EPackageCategoryType.Other)
            {
                return;
            }

            //不限制可以无限层
            GetNextCategory(m_CategoryData.Layer + 1);
        }

        public override void OnDestroy()
        {
        }

        private void GetNextCategory(int layer)
        {
            var lastPath     = m_CategoryData.CategoryPath.Replace("库/", "");
            var nextCategory = PackageHubHelper.GetNextCategoryData(this.ShowPackages, layer, lastPath);

            if (nextCategory is not { Count: > 0 }) return;

            foreach (var category in nextCategory.Keys)
            {
                var categoryPath = $"{m_CategoryData.CategoryPath}/{category}";

                var menuItem = new TreeMenuItem<PackageCategoryModule>(AutoTool, Tree, categoryPath, EditorIcons.Folder);
                menuItem.UserData = new PackageCategoryData
                {
                    CategoryType = m_CategoryType,
                    Category     = category,
                    CategoryPath = categoryPath,
                    Layer        = layer,
                    AllPackages  = nextCategory[category],
                };
            }
        }
    }
}
#endif
