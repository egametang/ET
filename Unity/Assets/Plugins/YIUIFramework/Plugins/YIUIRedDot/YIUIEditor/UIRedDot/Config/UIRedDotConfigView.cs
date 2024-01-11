#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;


namespace YIUIFramework.Editor
{
    [HideLabel]
    [HideReferenceObjectPicker]
    internal class UIRedDotConfigView : BaseCreateModule
    {
        internal UIRedDotModule    m_UIRedDotModule;
        internal RedDotConfigAsset m_RedDotConfigAsset;

        private EnumPrefs<ERedDotConfigViewIndexType> m_ERedDotConfigViewIndexTypePrefs =
            new EnumPrefs<ERedDotConfigViewIndexType>("AutoUIRedDotModule_ERedDotConfigViewIndexType", null,
                ERedDotConfigViewIndexType.Get);

        [EnumToggleButtons]
        [HideLabel]
        [ShowInInspector]
        [PropertyOrder(-100)]
        [OnValueChanged("OnValueChangedConfigViewIndex")]
        private ERedDotConfigViewIndexType m_ERedDotConfigViewIndexType = ERedDotConfigViewIndexType.Get;

        public UIRedDotConfigView(UIRedDotModule redDotModule)
        {
            m_UIRedDotModule    = redDotModule;
            m_RedDotConfigAsset = redDotModule.m_RedDotConfigAsset;
        }

        public override void Initialize()
        {
            m_ERedDotConfigViewIndexType = m_ERedDotConfigViewIndexTypePrefs.Value;
            NewAddConfigEditorData();
            InitConfigEditorDataList();
            m_UIRedDotModule.OnChangeViewSetIndex += OnChangeViewSetIndex;
        }

        public override void OnDestroy()
        {
            m_UIRedDotModule.OnChangeViewSetIndex -= OnChangeViewSetIndex;
            AutoSyncEditorData();
        }

        private void OnChangeViewSetIndex(UIRedDotModule.EUIRedDotViewType obj)
        {
            if (obj != UIRedDotModule.EUIRedDotViewType.Config)
            {
                AutoSyncEditorData();
            }
        }

        private void OnValueChangedConfigViewIndex()
        {
            m_ERedDotConfigViewIndexTypePrefs.Value = m_ERedDotConfigViewIndexType;
            if (m_ERedDotConfigViewIndexType != ERedDotConfigViewIndexType.Get)
            {
                //AutoSyncEditorData(); //暂时取消这个同步 频率很高
            }
        }

        #region 同步脏标

        //编辑器数据同步脏标
        internal bool m_EditorDataSyncDirty = false;

        //自动同步数据
        private void AutoSyncEditorData()
        {
            if (!m_EditorDataSyncDirty) return;

            UnityTipsHelper.CallBack($"关联配置数据 已修改是否同步 \n\n取消同步将会丢失所有修改!!!", UpdateConfigEditorDataToAsset);
        }

        #endregion

        #region 添加

        [OdinSerialize]
        [ShowInInspector]
        [BoxGroup("新增关联配置数据", centerLabel: true)]
        [ShowIf("m_ERedDotConfigViewIndexType", ERedDotConfigViewIndexType.Add)]
        [HideLabel]
        [HideReferenceObjectPicker]
        private UIRedDotConfigEditorData m_AddUIRedDotConfigEditorData;

        [GUIColor(0, 1, 0)]
        [Button("添加关联配置", 50)]
        [ShowIf("m_ERedDotConfigViewIndexType", ERedDotConfigViewIndexType.Add)]
        private void AddConfigEditorDataBtn()
        {
            var keyType = m_AddUIRedDotConfigEditorData.KeyType;

            if (keyType == ERedDotKeyType.None)
            {
                UnityTipsHelper.Show($"请设置一个合理的key");
                return;
            }

            if (EditorDataContains(keyType))
            {
                UnityTipsHelper.Show($"当前Key的相关配置已存在 无法重复添加 {keyType}");
                return;
            }

            m_AddUIRedDotConfigEditorData.ShowDeleteBtn = true;
            AddConfigEditorDataToList(m_AddUIRedDotConfigEditorData);

            NewAddConfigEditorData();

            m_EditorDataSyncDirty = true;
            UnityTipsHelper.Show($"成功添加 {keyType}");
        }

        private void NewAddConfigEditorData()
        {
            m_AddUIRedDotConfigEditorData = new UIRedDotConfigEditorData(this)
            {
                ShowDeleteBtn = false,
            };
            m_AddUIRedDotConfigEditorData.ParentList.Add(new UIRedDotKeyEditorData(m_AddUIRedDotConfigEditorData));
        }

        #endregion

        #region 查看

        [GUIColor(0.4f, 0.8f, 1)]
        [Button("同步到Asset配置", 50)]
        [ShowIf("m_ERedDotConfigViewIndexType", ERedDotConfigViewIndexType.Get)]
        [PropertyOrder(-1)]
        [EnableIf("m_EditorDataSyncDirty")]
        private void UpdateConfigEditorDataToAsset()
        {
            m_EditorDataSyncDirty = false;
            DeserializationEditorData();
            InitConfigEditorDataList();
            UnityTipsHelper.Show($"同步完成");
        }

        [TableList(DrawScrollView = true, AlwaysExpanded = true, IsReadOnly = true)]
        [OdinSerialize]
        [BoxGroup("所有关联配置数据", centerLabel: true)]
        [HideLabel]
        [ShowIf("m_ERedDotConfigViewIndexType", ERedDotConfigViewIndexType.Get)]
        private List<UIRedDotConfigEditorData> m_RedDotConfigEditorDataList = new List<UIRedDotConfigEditorData>();

        private HashSet<ERedDotKeyType> m_HashRedDotKeyType = new HashSet<ERedDotKeyType>();

        #region 初始化 数据 根据存储的Asset

        private void InitConfigEditorDataList()
        {
            m_RedDotConfigEditorDataList.Clear();
            m_HashRedDotKeyType.Clear();
            foreach (var data in m_RedDotConfigAsset.AllRedDotConfigDic.Values)
            {
                var editorData = NewEditorDataByRuntimeData(data);
                AddConfigEditorDataToList(editorData);
            }
        }

        private UIRedDotConfigEditorData NewEditorDataByRuntimeData(RedDotConfigData data)
        {
            var editorData = new UIRedDotConfigEditorData(this)
            {
                Id            = (int)data.Key,
                KeyType       = data.Key,
                SwitchTips    = data.SwitchTips,
                ShowDeleteBtn = true
            };

            editorData.ParentList = GetParentListByRuntimeData(data, editorData);

            return editorData;
        }

        private List<UIRedDotKeyEditorData> GetParentListByRuntimeData(RedDotConfigData         data,
                                                                       UIRedDotConfigEditorData editorData)
        {
            var parentList = new List<UIRedDotKeyEditorData>();

            foreach (var key in data.ParentList)
            {
                parentList.Add(new UIRedDotKeyEditorData(editorData)
                {
                    Id      = (int)key,
                    KeyType = key,
                });
            }

            return parentList;
        }

        #endregion

        #region 反序列化当前数据到存储数据

        private void DeserializationEditorData()
        {
            var list = GetRuntimeDataListToEditorData();
            list.Sort(SortRuntimeData);
            m_RedDotConfigAsset.SetAllRedDotConfigList(list);
        }

        private int SortRuntimeData(RedDotConfigData x, RedDotConfigData y)
        {
            return x.Key < y.Key ? -1 : 1;
        }

        private List<RedDotConfigData> GetRuntimeDataListToEditorData()
        {
            var runtimeDataList = new List<RedDotConfigData>();

            foreach (var data in m_RedDotConfigEditorDataList)
            {
                var runtimeData = NewRuntimeDataByEditorData(data);
                runtimeDataList.Add(runtimeData);
            }

            return runtimeDataList;
        }

        private RedDotConfigData NewRuntimeDataByEditorData(UIRedDotConfigEditorData data)
        {
            var runtimeData = new RedDotConfigData()
            {
                Key        = data.KeyType,
                ParentList = GetParentListByEditorData(data),
                SwitchTips = data.SwitchTips,
            };

            return runtimeData;
        }

        private List<ERedDotKeyType> GetParentListByEditorData(UIRedDotConfigEditorData data)
        {
            var parentList = new List<ERedDotKeyType>();

            foreach (var parentData in data.ParentList)
            {
                if (parentData.KeyType == ERedDotKeyType.None)
                {
                    continue;
                }

                parentList.Add(parentData.KeyType);
            }

            return parentList;
        }

        #endregion

        private bool AddConfigEditorDataToList(UIRedDotConfigEditorData data)
        {
            if (m_HashRedDotKeyType.Contains(data.KeyType))
            {
                Debug.LogError($"已存在 无法重复添加");
                return false;
            }

            m_RedDotConfigEditorDataList.Add(data);
            m_HashRedDotKeyType.Add(data.KeyType);
            return true;
        }

        internal bool EditorDataContains(ERedDotKeyType keyType)
        {
            return m_HashRedDotKeyType.Contains(keyType);
        }

        internal void DeleteConfigEditorData(UIRedDotConfigEditorData data)
        {
            m_RedDotConfigEditorDataList.Remove(data);
            m_HashRedDotKeyType.Remove(data.KeyType);
        }

        #endregion

        #region DAG

        [ShowIf("m_ERedDotConfigViewIndexType", ERedDotConfigViewIndexType.Get)]
        [Button("额外检查是否有循环引用", 30)]
        [PropertyOrder(-100)]
        public void CheckDataCycles()
        {
            var redDotConfigDAG = new UIRedDotConfigDAG();

            foreach (ERedDotKeyType key in Enum.GetValues(typeof(ERedDotKeyType)))
            {
                redDotConfigDAG.AddNode(key);
            }

            foreach (var data in m_RedDotConfigEditorDataList)
            {
                foreach (var parentData in data.ParentList)
                {
                    redDotConfigDAG.AddEdge(parentData.KeyType, data.KeyType);
                }
            }

            var result = redDotConfigDAG.Check();
            UnityTipsHelper.Show(result ? "安全 无循环嵌套" : "有循环嵌套 请详细检查");
        }

        #endregion

        [HideLabel]
        private enum ERedDotConfigViewIndexType
        {
            [LabelText("增")]
            Add = 1,

            [LabelText("删/改/查")]
            Get = 2,
        }
    }
}
#endif