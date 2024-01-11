#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


namespace YIUIFramework.Editor
{
    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    internal class UIRedDotConfigEditorData
    {
        [VerticalGroup("Key")]
        [HideLabel]
        [TableColumnWidth(400, resizable: false)]
        [DisableIf("ShowDeleteBtn")]
        [OnValueChanged("OnValueChangedKeyType")]
        [ShowInInspector]
        [OdinSerialize]
        internal ERedDotKeyType KeyType;

        [VerticalGroup("Key")]
        [LabelText("ID")]
        [ReadOnly]
        [ShowInInspector]
        [OdinSerialize]
        internal int Id;

        [VerticalGroup("Key")]
        [LabelText("可开关提示")]
        [Tooltip("true = 玩家可开关 false = 不可开关 (永久提示)")]
        [ShowInInspector]
        [OnValueChanged("OnValueChangedSwitchTips")]
        [OdinSerialize]
        internal bool SwitchTips = true;

        [VerticalGroup("所有父级列表")]
        [HideLabel]
        [TableList(DrawScrollView = true, AlwaysExpanded = true, HideToolbar = true, MaxScrollViewHeight = 100,
            MinScrollViewHeight = 100)]
        [OdinSerialize]
        [ShowInInspector]
        [OnValueChanged("OnValueChangedParentList")]
        [PropertyOrder(1001)]
        internal List<UIRedDotKeyEditorData> ParentList = new List<UIRedDotKeyEditorData>();

        [VerticalGroup("所有父级列表")]
        [Button("添加父级", 10)]
        [GUIColor(0.7f, 0.4f, 0.8f)]
        [PropertyOrder(1000)]
        private void AddParentData()
        {
            ParentList.Add(new UIRedDotKeyEditorData(this));
        }

        [GUIColor(1, 1, 0)]
        [VerticalGroup("删除")]
        [TableColumnWidth(50, resizable: false)]
        [Button("删除", 90)]
        [ShowIf("ShowDeleteBtn")]
        [PropertyOrder(10000)]
        private void DeleteData()
        {
            UnityTipsHelper.CallBack($"确定移除当前配置数据? {KeyType}",
                () =>
                {
                    m_UIRedDotConfigView.m_EditorDataSyncDirty = true;
                    m_UIRedDotConfigView?.DeleteConfigEditorData(this);
                });
        }

        [HideInInspector]
        [OdinSerialize]
        internal bool ShowDeleteBtn = true;

        private UIRedDotConfigView m_UIRedDotConfigView;

        internal UIRedDotConfigEditorData(UIRedDotConfigView configView)
        {
            m_UIRedDotConfigView = configView;
        }

        private void OnValueChangedSwitchTips()
        {
            if (ShowDeleteBtn)
            {
                m_UIRedDotConfigView.m_EditorDataSyncDirty = true;
            }
        }

        private void OnValueChangedKeyType()
        {
            if (m_UIRedDotConfigView.EditorDataContains(KeyType))
            {
                KeyType = ERedDotKeyType.None;
                UnityTipsHelper.Show($"当前配置已存在  {KeyType}");
            }

            Id = (int)KeyType;
        }

        #region 检查 ParentList Key设定是否合理

        private void OnValueChangedParentList()
        {
            CheckParentList();
        }

        internal void CheckParentList(bool showTips = true)
        {
            if (ShowDeleteBtn)
            {
                m_UIRedDotConfigView.m_EditorDataSyncDirty = true;
            }

            var sb = SbPool.Get();

            CheckParentListHaveSelf(sb);
            CheckParentListHaveEqual(sb);
            CheckParentListParentHaveSelf(sb);

            var tipsContent = SbPool.PutAndToStr(sb);
            if (!string.IsNullOrEmpty(tipsContent))
            {
                if (showTips)
                {
                    UnityTipsHelper.Show(tipsContent);
                }
                else
                {
                    Debug.LogError(tipsContent);
                }
            }
        }

        //是否有自身
        private void CheckParentListHaveSelf(StringBuilder sb)
        {
            for (int i = 0; i < ParentList.Count; i++)
            {
                var data = ParentList[i];
                if (data.KeyType == KeyType)
                {
                    ParentList.Remove(data);
                    sb.AppendLine($"当前数据与自身相同 不能循环引用自身 已移除 {data.KeyType}");
                }
            }
        }

        //是否有相同数据
        private void CheckParentListHaveEqual(StringBuilder sb)
        {
            for (int i = 0; i < ParentList.Count; i++)
            {
                var data = ParentList[i];

                for (int j = 0; j < ParentList.Count; j++)
                {
                    var data2 = ParentList[j];

                    if (data == data2)
                    {
                        continue;
                    }

                    if (data.KeyType == data2.KeyType)
                    {
                        ParentList.Remove(data);
                        sb.AppendLine($"已存在相同数据 已移除 {data.KeyType}");
                        CheckParentListHaveEqual(sb);
                        return;
                    }
                }
            }
        }

        //判断父级的父级的父级一直递归 他们中的父级是不是有我 否则会形成嵌套循环
        private void CheckParentListParentHaveSelf(StringBuilder sb)
        {
            for (int i = 0; i < ParentList.Count; i++)
            {
                var data = ParentList[i];

                var parentConfigData = m_UIRedDotConfigView?.m_RedDotConfigAsset?.GetConfigData(data.KeyType);
                if (parentConfigData == null)
                {
                    continue;
                }

                if (CheckParentConfigDataHaveSelf(parentConfigData))
                {
                    ParentList.Remove(data);
                    sb.AppendLine($"我的当前父级{data.KeyType}  递归上父级中存在{KeyType} 无法循环引用 已移除 请规避循环问题");
                    CheckParentListParentHaveSelf(sb);
                    return;
                }
            }
        }

        private bool CheckParentConfigDataHaveSelf(RedDotConfigData configData)
        {
            foreach (var parentParentKey in configData.ParentList)
            {
                if (parentParentKey == KeyType)
                {
                    return true;
                }

                var parentConfigData = m_UIRedDotConfigView?.m_RedDotConfigAsset?.GetConfigData(parentParentKey);
                if (parentConfigData == null)
                {
                    continue;
                }

                if (CheckParentConfigDataHaveSelf(parentConfigData))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
#endif