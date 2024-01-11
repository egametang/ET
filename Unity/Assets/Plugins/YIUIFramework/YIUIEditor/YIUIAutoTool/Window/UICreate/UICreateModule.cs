#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    public static class UICreateModule
    {
        internal static void Create(UIBindCDETable cdeTable, bool refresh, bool tips)
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            /*
            //留在这里看的 方便以后查API
            //当这个是个资源的时候 存在磁盘中
            var is0 = UnityEditor.EditorUtility.IsPersistent(cdeTable);
            //返回></para>如果对象是Prefab的一部分，则为True/返回>Functionl
            var is1= UnityEditor.PrefabUtility.IsPartOfAnyPrefab(cdeTable);
            //</para> . 0如果对象是不能编辑的Prefab的一部分，则为True
            var is2= UnityEditor.PrefabUtility.IsPartOfImmutablePrefab(cdeTable);
            //>如果给定对象是Model Prefab Asset或Model Prefab实例的一部分，则返回true
            var is3= UnityEditor.PrefabUtility.IsPartOfModelPrefab(cdeTable);
            //<摘要></para>如果给定对象是预制资源的一部分，则返回true
            var is4= UnityEditor.PrefabUtility.IsPartOfPrefabAsset(cdeTable);
            //摘要></para>如果给定对象是Prefab实例的一部分，则返回true
            var is5= UnityEditor.PrefabUtility.IsPartOfPrefabInstance(cdeTable);
            //a>如果给定对象是常规预制实例或预制资源的一部分，则返回true。</para>
            var is6= UnityEditor.PrefabUtility.IsPartOfRegularPrefab(cdeTable);
            //>如果给定对象是Prefab Variant资产或Prefab Variant实例的一部分，则返回true。
            var is7= UnityEditor.PrefabUtility.IsPartOfVariantPrefab(cdeTable);
            //>如果给定对象是Prefab实例的一部分，而不是资产的一部分，则返回true。</pa
            var is8= UnityEditor.PrefabUtility.IsPartOfNonAssetPrefabInstance(cdeTable);
            //>这个对象是Prefab的一部分，不能应用吗?
            var is9= UnityEditor.PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(cdeTable);
            */

            var checkInstance = PrefabUtility.IsPartOfPrefabInstance(cdeTable);
            if (checkInstance)
            {
                UnityTipsHelper.ShowErrorContext(cdeTable, $"不能对实体进行操作  必须进入预制体编辑!!!");
                return;
            }

            var checkAsset = PrefabUtility.IsPartOfPrefabAsset(cdeTable);
            if (!checkAsset)
            {
                UnityTipsHelper.ShowErrorContext(cdeTable,
                    $"1: 必须是预制体 2: 不能在Hierarchy面板中使用 必须在Project面板下的预制体原件才能使用使用 ");
                return;
            }

            if (!cdeTable.AutoCheck())
            {
                return;
            }

            //component信息
            var createBaseData = new UICreateBaseData
            {
                AutoRefresh   = false,
                ShowTips      = tips,
                Namespace     = UIStaticHelper.UINamespace,
                PkgName       = cdeTable.PkgName,
                ResName       = cdeTable.ResName,
                CodeType      = cdeTable.UICodeType,
                PanelLayer    = cdeTable.PanelLayer,
                Variables     = UICreateVariables.Get(cdeTable),
                UIFriend      = UICreateBind.GetFriend(cdeTable),
                UIBase        = UICreateBind.GetBase(cdeTable),
                UIBind        = UICreateBind.GetBind(cdeTable),
                UIUnBind      = UICreateBind.GetUnBind(cdeTable),
                VirtualMethod = UICreateMethod.Get(cdeTable),
                PanelViewEnum = UICreatePanelViewEnum.Get(cdeTable),
            };

            //自定义的component
            new UICreateComponentCode(out var resultComponent, YIUIAutoTool.Author, createBaseData);

            //生成的system
            new UICreateSystemGenCode(out var resultSystemGen, YIUIAutoTool.Author, createBaseData);

            //system信息
            var createSystemData = new UICreateSystemData
            {
                AutoRefresh = false,
                ShowTips    = tips,
                Namespace   = UIStaticHelper.UINamespace,
                PkgName     = cdeTable.PkgName,
                ResName     = cdeTable.ResName,
                OverrideDic = UICreateMethod.GetSystemEventOverrideDic(cdeTable),
                CoverDic    = UICreateMethod.CoverSystemDefaultEventDic(cdeTable),
            };

            //生成的component
            //自定义的system
            //目前看上去3个都一样 是特意设定的 以后可独立扩展
            if (cdeTable.UICodeType == EUICodeType.Panel)
            {
                new UICreatePanelSystemCode(out var resultSystem, YIUIAutoTool.Author, createSystemData);
                if (!resultSystem) return;
                new UICreateComponentGenCode(out var resultComponentBase, YIUIAutoTool.Author, createBaseData);
                if (!resultComponentBase) return;
            }
            else if (cdeTable.UICodeType == EUICodeType.View)
            {
                new UICreateViewSystemCode(out var resultSystem, YIUIAutoTool.Author, createSystemData);
                if (!resultSystem) return;
                new UICreateComponentGenCode(out var resultComponentBase, YIUIAutoTool.Author, createBaseData);
                if (!resultComponentBase) return;
            }
            else if (cdeTable.UICodeType == EUICodeType.Common)
            {
                new UICreateCommonSystemCode(out var resultSystem, YIUIAutoTool.Author, createSystemData);
                if (!resultSystem) return;
                new UICreateCommonComponentGenCode(out var resultComponentBase, YIUIAutoTool.Author, createBaseData);
                if (!resultComponentBase) return;
            }
            else
            {
                Debug.LogError($"是新增了 新类型嘛????? {cdeTable.UICodeType}");
            }

            if (refresh)
                AssetDatabase.Refresh();
        }

        private static string GetRegionEvent(UIBindCDETable cdeTable)
        {
            if (cdeTable.EventTable == null || cdeTable.EventTable.EventDic == null)
                return "";
            return cdeTable.EventTable.EventDic.Count <= 0
                    ? ""
                    : "        #region YIUIEvent开始\r\n\r\n        #endregion YIUIEvent结束";
        }

        internal static bool InitVoName(UIBindCDETable cdeTable)
        {
            var path    = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cdeTable);
            var pkgName = GetPkgName(path);
            if (string.IsNullOrEmpty(pkgName))
            {
                UnityTipsHelper.ShowErrorContext(cdeTable,
                    $"没有找到模块名 请在预制体上使用 且 必须在指定的文件夹下才可使用 {UIStaticHelper.UIProjectResPath}");
                return false;
            }

            cdeTable.PkgName = Regex.Replace(pkgName, NameUtility.NameRegex, "");
            cdeTable.ResName = Regex.Replace(cdeTable.name, NameUtility.NameRegex, "");
            if (cdeTable.ResName != cdeTable.name)
            {
                cdeTable.name = cdeTable.ResName;
                AssetDatabase.RenameAsset(path, cdeTable.ResName);
            }

            return true;
        }

        private static string GetPkgName(string path, string currentName = "")
        {
            if (!path.Replace("\\", "/").Contains(UIStaticHelper.UIProjectResPath))
            {
                return null;
            }

            var parentInfo = System.IO.Directory.GetParent(path);
            if (parentInfo == null)
            {
                return currentName;
            }

            if (parentInfo.Name == UIStaticHelper.UIProjectName)
            {
                return currentName;
            }

            return GetPkgName(parentInfo.FullName, parentInfo.Name);
        }

        //收集所有公共组件
        internal static void RefreshChildCdeTable(UIBindCDETable cdeTable)
        {
            cdeTable.AllChildCdeTable.Clear();
            AddCdeTable(ref cdeTable.AllChildCdeTable, cdeTable.transform);
            CheckAddCdeTable(ref cdeTable.AllChildCdeTable,cdeTable);
        }

        //如果自己是panel 则还需要额外检查 是不是把自己的view给收集进去了
        private static void CheckAddCdeTable(ref List<UIBindCDETable> addCdeTable, UIBindCDETable cdeTable)
        {
            if (cdeTable.UICodeType != EUICodeType.Panel && !cdeTable.IsSplitData)
                return;
            
            for (var i = addCdeTable.Count - 1; i >= 0; i--)
            {
                var targetTable =addCdeTable[i];
                var parent      = (RectTransform)targetTable.gameObject.transform.parent;
                var parentName  = parent.name;
                //这里使用的是强判断 如果使用|| 可以弱判断根据需求  如果遵守View规则是没有问题的
                if (parentName.Contains(UIStaticHelper.UIParentName) && parentName.Contains(targetTable.gameObject.name))
                {
                    //常驻View 不需要移除
                    if (cdeTable.PanelSplitData.AllCommonView.Contains(parent)) break;
                    addCdeTable.RemoveAt(i);
                }
            }
        }
        
        private static void AddCdeTable(ref List<UIBindCDETable> cdeTable, Transform transform)
        {
            var childCount = transform.childCount;
            if (childCount <= 0) return;

            for (var i = childCount - 1; i >= 0; i--)
            {
                var child    = transform.GetChild(i);
                var childCde = child.GetComponent<UIBindCDETable>();

                if (childCde == null)
                {
                    AddCdeTable(ref cdeTable, child);
                    continue;
                }

                if (string.IsNullOrEmpty(childCde.PkgName) || string.IsNullOrEmpty(childCde.ResName)) continue;
                if (childCde.UICodeType == EUICodeType.Panel)
                {
                    Debug.LogError($"{transform.name} 公共组件嵌套了 其他面板 这是不允许的 {childCde.ResName} 已跳过忽略");
                    continue;
                }

                var newName = Regex.Replace(childCde.name, NameUtility.NameRegex, "");
                if (childCde.name != newName)
                {
                    childCde.name = newName;
                }

                cdeTable.Add(childCde);
            }
        }
    }
}
#endif