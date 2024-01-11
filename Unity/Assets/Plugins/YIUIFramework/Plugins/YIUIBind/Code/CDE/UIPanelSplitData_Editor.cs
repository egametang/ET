#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace YIUIFramework
{
    public sealed partial class UIPanelSplitData
    {
        [OdinSerialize]
        [LabelText("生成通用界面枚举")]
        [ShowIf("ShowCreatePanelViewEnum")]
        internal bool CreatePanelViewEnum = true;

        internal bool ShowCreatePanelViewEnum()
        {
            return (AllCommonView.Count + AllCreateView.Count + AllPopupView.Count) >= 1;
        }

        private bool ShowCheckBtn()
        {
            if (Panel == null)
            {
                return false;
            }

            if (!Panel.name.EndsWith(UIStaticHelper.UISource))
            {
                return false;
            }

            return true;
        }

        [GUIColor(0, 1, 1)]
        [Button("检查拆分数据", 30)]
        [ShowIf("ShowCheckBtn")]
        private void AutoCheckBtn()
        {
            AutoCheck();
        }

        internal bool AutoCheck()
        {
            if (!ResetParent()) return false;

            if (!CheckPanelName()) return false;

            CheckViewName(AllCommonView);
            CheckViewName(AllCreateView);
            CheckViewName(AllPopupView);

            CheckViewParent(AllCommonView, AllViewParent);
            CheckViewParent(AllCreateView, AllViewParent);
            CheckViewParent(AllPopupView, AllPopupViewParent);

            var hashList = new HashSet<RectTransform>();
            CheckRepetition(ref hashList, AllCommonView);
            CheckRepetition(ref hashList, AllCreateView);
            CheckRepetition(ref hashList, AllPopupView);

            return true;
        }

        private bool ResetParent()
        {
            if (Panel == null)
            {
                Debug.LogError($"没有找到 Panel");
                return false;
            }

            if (AllViewParent == null || AllViewParent.name != UIStaticHelper.UIAllViewParentName)
            {
                AllViewParent = Panel.transform.FindChildByName(UIStaticHelper.UIAllViewParentName)
                                     .GetComponent<RectTransform>();
            }

            if (AllViewParent == null)
            {
                Debug.LogError($"没有找到 {Panel.name} {UIStaticHelper.UIAllViewParentName}  这是必须存在的组件 你可以不用 但是不能没有");
                return false;
            }

            if (AllPopupViewParent == null || AllPopupViewParent.name != UIStaticHelper.UIAllPopupViewParentName)
            {
                AllPopupViewParent = Panel.transform.FindChildByName(UIStaticHelper.UIAllPopupViewParentName)
                                          .GetComponent<RectTransform>();
            }

            if (AllPopupViewParent == null)
            {
                Debug.LogError($"没有找到 {Panel.name} {UIStaticHelper.UIAllPopupViewParentName}  这是必须存在的组件 你可以不用 但是不能没有");
                return false;
            }

            return true;
        }

        private bool CheckPanelName()
        {
            var qualifiedName = NameUtility.ToFirstUpper(Panel.name);
            if (Panel.name != qualifiedName)
            {
                Panel.name = qualifiedName;
            }

            if (Panel.name == UIStaticHelper.UIYIUIPanelSourceName)
            {
                Debug.LogError($"当前是默认名称 请手动修改名称 Xxx{UIStaticHelper.UIPanelSourceName}");
                return false;
            }

            if (!Panel.name.EndsWith($"{UIStaticHelper.UIPanelSourceName}"))
            {
                Debug.LogError($"{Panel.name} 命名必须以 {UIStaticHelper.UIPanelSourceName} 结尾 请勿随意修改");
                return false;
            }

            return true;
        }

        //命名检查
        private void CheckViewName(List<RectTransform> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var current = list[i];
                if (current == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                var qualifiedName = NameUtility.ToFirstUpper(current.name);
                if (current.name != qualifiedName)
                {
                    current.name = qualifiedName;
                }

                if (current.name == UIStaticHelper.UIYIUIViewParentName)
                {
                    Debug.LogError($"当前是默认名称 请手动修改名称 Xxx{UIStaticHelper.UIViewParentName}");
                    list.RemoveAt(i);
                    continue;
                }

                if (!current.name.EndsWith(UIStaticHelper.UIViewParentName))
                {
                    Debug.LogError($"{current.name} 命名必须以 {UIStaticHelper.UIViewParentName} 结尾 请勿随意修改");
                    list.RemoveAt(i);
                    continue;
                }

                var viewName = current.name.Replace(UIStaticHelper.UIParentName, "");
                var viewCde  = current.GetComponentInChildren<UIBindCDETable>();
                
                if (viewCde == null)
                {
                    //如果这个子物体被隐藏了
                    if (current.transform.childCount >= 1)
                    {
                        var firstChild = current.transform.GetChild(0);
                        viewCde = firstChild.GetComponent<UIBindCDETable>();
                    }
                }

                if (viewCde == null)
                {
                    Debug.LogError($" {current.name} 父物体下必须有View  但是未找到View 请使用 右键 YIUI/Create UIView 创建符合要求的结构");
                    list.RemoveAt(i);
                    continue;
                }
                
                viewCde.gameObject.name = viewName;
            }
        }

        //检查null / 父级
        private void CheckViewParent(List<RectTransform> list, RectTransform parent)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                var current = list[i];

                var parentP = current.parent;
                if (parentP == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                if (parentP != parent)
                {
                    list.RemoveAt(i);

                    //因为只有2个父级 所以如果不是这个就会自动帮你移动到另外一个上面
                    //如果多了还是不要自动了
                    var currentParentName = parentP.name;
                    if (currentParentName == UIStaticHelper.UIAllViewParentName)
                    {
                        AllCreateView.Add(current);
                    }
                    else if (currentParentName == UIStaticHelper.UIAllPopupViewParentName)
                    {
                        AllPopupView.Add(current);
                    }
                }
            }
        }

        //检查重复
        private void CheckRepetition(ref HashSet<RectTransform> hashList, List<RectTransform> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var current = list[i];
                if (hashList.Contains(current))
                {
                    list.RemoveAt(i);
                    Debug.LogError($"{Panel.name} / {current.name} 重复存在 已移除 请检查");
                }
                else
                {
                    hashList.Add(current);
                }
            }
        }
    }
}
#endif