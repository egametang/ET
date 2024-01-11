#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


namespace YIUIFramework.Editor
{
    [HideLabel]
    [HideReferenceObjectPicker]
    internal class UIRedDotKeyView : BaseCreateModule
    {
        private UIRedDotModule m_UIRedDotModule;
        private RedDotKeyAsset m_RedDotKeyAsset;

        private EnumPrefs<ERedDotKeyViewIndexType> m_ERedDotKeyViewIndexTypePrefs =
            new EnumPrefs<ERedDotKeyViewIndexType>("AutoUIRedDotModule_ERedDotKeyViewIndexTypePrefs", null,
                ERedDotKeyViewIndexType.Add);

        [EnumToggleButtons]
        [HideLabel]
        [ShowInInspector]
        [OnValueChanged("OnValueChangedKeyViewIndex")]
        private ERedDotKeyViewIndexType m_ERedDotKeyViewIndexType = ERedDotKeyViewIndexType.Add;

        [HideInInspector]
        private ERedDotKeyViewIndexType m_LastKeyViewIndex = ERedDotKeyViewIndexType.Add;

        private void OnValueChangedKeyViewIndex()
        {
            m_ERedDotKeyViewIndexTypePrefs.Value = m_ERedDotKeyViewIndexType;

            if (m_ERedDotKeyViewIndexType == ERedDotKeyViewIndexType.Add)
            {
                return;
            }

            if (m_LastKeyViewIndex != ERedDotKeyViewIndexType.Add)
            {
                return;
            }

            if (m_WaitCreate)
            {
                CreateKeyEnumClass();
            }
        }

        [OdinSerialize]
        [ReadOnly]
        [PropertyOrder(1000000)]
        [BoxGroup("预览", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        private IReadOnlyDictionary<int, RedDotKeyData> m_AllRedDotKey;

        public UIRedDotKeyView(UIRedDotModule redDotModule)
        {
            m_UIRedDotModule = redDotModule;
            m_RedDotKeyAsset = redDotModule.m_RedDotKeyAsset;
            m_AllRedDotKey   = m_RedDotKeyAsset.AllRedDotDic;
        }

        public override void Initialize()
        {
            NewAddRedDataKeyData();
            m_ERedDotKeyViewIndexType = m_ERedDotKeyViewIndexTypePrefs.Value;
            OnValueChangedKeyViewIndex();
            m_UIRedDotModule.OnChangeViewSetIndex += OnChangeViewSetIndex;
            CheckContrastRedDotKey();
        }

        private void CheckContrastRedDotKey()
        {
            var enumKeyList = GetERedDotKeyTypeList();
            if (m_AllRedDotKey.Count != enumKeyList.Count)
            {
                UnityTipsHelper.CallBack($"当前配置数据 长度:{m_AllRedDotKey.Count} 与 枚举数据 长度:{enumKeyList.Count}  不一致 请重新生成",
                    CreateKeyEnumClass);
            }
        }

        private List<ERedDotKeyType> GetERedDotKeyTypeList()
        {
            var list = new List<ERedDotKeyType>();
            foreach (ERedDotKeyType key in Enum.GetValues(typeof(ERedDotKeyType)))
            {
                if (key != ERedDotKeyType.None)
                {
                    list.Add(key);
                }
            }

            return list;
        }

        private void OnChangeViewSetIndex(UIRedDotModule.EUIRedDotViewType obj)
        {
            if (obj != UIRedDotModule.EUIRedDotViewType.Key)
            {
                if (m_WaitCreate)
                {
                    CreateKeyEnumClass();
                }
            }
        }

        public override void OnDestroy()
        {
            m_UIRedDotModule.OnChangeViewSetIndex -= OnChangeViewSetIndex;
            if (m_WaitCreate)
            {
                CreateKeyEnumClass();
            }
        }

        private bool m_WaitCreate = false;

        [BoxGroup("生成", centerLabel: true)]
        [Button("自动生成Key枚举类", 50)]
        [ShowIf("m_WaitCreate")]
        [PropertyOrder(10000)]
        [GUIColor(0.4f, 0.8f, 1)]
        private void CreateKeyEnumClass()
        {
            m_WaitCreate = false;

            var createData = new UICreateRedDotKeyData
            {
                AutoRefresh = true,
                ShowTips    = true,
                ClassPath   = m_UIRedDotModule.UIRedDotKeyClassPath,
                Content     = UICreateRedDotKeyGet.Get(m_RedDotKeyAsset.GetDataList()),
            };

            new UICreateRedDotKeyCode(out var resultBase, YIUIAutoTool.Author, createData);

            if (!resultBase) return;
        }

        #region 增删改查

        #region 增

        [HideLabel]
        [ShowInInspector]
        [ShowIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Add)]
        [PropertyOrder(2001)]
        [BoxGroup("添加数据", centerLabel: true)]
        private RedDotKeyData m_AddKeyData;

        [ButtonGroup("添加")]
        [Button("添加(生成)", 50)]
        [GUIColor(0, 1, 0)]
        [ShowIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Add)]
        [PropertyOrder(2002)]
        private void AddKeyCreateBtn()
        {
            AddKey();
        }

        [ButtonGroup("添加")]
        [Button("添加(不生成)", 50)]
        [GUIColor(1, 1, 0)]
        [ShowIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Add)]
        [PropertyOrder(2002)]
        private void AddKeyBtn()
        {
            var result = AddKey(false);
            if (result)
            {
                m_WaitCreate = true;
            }
        }

        private bool AddKey(bool createKey = true)
        {
            var (result, tips) = m_RedDotKeyAsset.AddKey(m_AddKeyData);
            NewAddRedDataKeyData();
            if (!result)
            {
                UnityTipsHelper.Show($"{tips} 建议可以直接参考默认ID");
            }
            else
            {
                if (createKey)
                {
                    CreateKeyEnumClass();
                }
                else
                {
                    UnityTipsHelper.Show(tips);
                }
            }

            return result;
        }

        private void NewAddRedDataKeyData()
        {
            var id = m_RedDotKeyAsset.GetSafetyId();
            m_AddKeyData = new RedDotKeyData(id);
        }

        #endregion

        #region 删

        //TODO 目前删除/修改 前检测只做了配置档
        //最终目标应该还额外扫描其他的
        //1 预制体 关联了那些红点的预制体 上面会关联Key 这个对应的修改删除
        //2 代码 某些代码会涉及到 获取 修改等 也对应的修改删除

        [ShowInInspector]
        [ShowIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Delete)]
        [LabelText("删除的ID")]
        [PropertyOrder(3001)]
        [BoxGroup("删除数据", centerLabel: true)]
        private int m_DeleteId = 0;

        [Button("删除", 50)]
        [GUIColor(1f, 0.3f, 0.3f)]
        [ShowIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Delete)]
        [PropertyOrder(3002)]
        private void DeleteKeyBtn()
        {
            if (!m_RedDotKeyAsset.ContainsKey(m_DeleteId))
            {
                UnityTipsHelper.Show($"当前ID不存在 无法移除 {m_DeleteId}");
                return;
            }

            var linkData =
                UIRedDotKeyEditorHelper.GetCheckKeyLink(m_UIRedDotModule.m_RedDotConfigAsset,
                    (ERedDotKeyType)m_DeleteId);

            if (linkData.ConfigSet || linkData.LinkKey.Count >= 1)
            {
                var configSetTips = linkData.ConfigSet ? "已配置" : "未配置";
                var linkTips      = $"关联引用 {linkData.LinkKey.Count}";
                foreach (var linkKey in linkData.LinkKey)
                {
                    linkTips += $"\n已被 {linkKey} 设定为父级";
                }

                var unityTips = $"{linkData.Key} 已被使用 确定删除吗?\n\n{configSetTips}\n{linkTips}\n\n同步的将会删除所有关联数据";
                UnityTipsHelper.CallBack(unityTips, () =>
                {
                    DeleteKey((result) =>
                    {
                        if (!result) return;
                        UIRedDotKeyEditorHelper.RemoveKeyByLink(m_UIRedDotModule.m_RedDotConfigAsset, linkData);
                        CreateKeyEnumClass();
                        YIUIAutoTool.CloseWindowRefresh();
                    }, false);
                });
            }
            else
            {
                DeleteKey((result) =>
                {
                    if (!result) return;
                    CreateKeyEnumClass();
                });
            }
        }

        private void DeleteKey(Action<bool> resultAction = null, bool showTips = true)
        {
            var (result, tips) = m_RedDotKeyAsset.RemoveKey(m_DeleteId);
            if (showTips)
                UnityTipsHelper.Show(tips);
            m_DeleteId = 0;
            resultAction?.Invoke(result);
        }

        #endregion

        #region 改

        //TODO 目前删除/修改 前检测只做了配置档
        //最终目标应该还额外扫描其他的
        //1 预制体 关联了那些红点的预制体 上面会关联Key 这个对应的修改删除
        //2 代码 某些代码会涉及到 获取 修改等 也对应的修改删除

        [HideLabel]
        [ShowInInspector]
        [ShowIf("ShowIfChangeKeyData")]
        [PropertyOrder(4001)]
        [BoxGroup("修改数据", centerLabel: true)]
        private RedDotKeyData m_ChangeKeyData;

        private bool ShowIfChangeKeyData()
        {
            if (m_ERedDotKeyViewIndexType != ERedDotKeyViewIndexType.Change)
            {
                return false;
            }

            return m_ChangeKeyData != null;
        }

        [Button("修改", 50)]
        [GUIColor(0.7f, 0.4f, 0.8f)]
        [ShowIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Change)]
        [PropertyOrder(4002)]
        private void ChangeKeyBtn()
        {
            if (m_ChangeKeyData == null)
            {
                UnityTipsHelper.Show($"请先 查询>>设置  需要修改的数据");
                return;
            }

            var linkData =
                UIRedDotKeyEditorHelper.GetCheckKeyLink(m_UIRedDotModule.m_RedDotConfigAsset,
                    (ERedDotKeyType)m_GetKeyData.Id);

            if (linkData.ConfigSet || linkData.LinkKey.Count >= 1)
            {
                var configSetTips = linkData.ConfigSet ? "已配置" : "未配置";
                var linkTips      = $"关联引用 {linkData.LinkKey.Count}";
                foreach (var linkKey in linkData.LinkKey)
                {
                    linkTips += $"\n已被 {linkKey} 设定为父级";
                }

                var unityTips = $"{linkData.Key} 已被使用 确定修改吗?\n\n{configSetTips}\n{linkTips}\n\n同步的将会修改所有关联数据";
                UnityTipsHelper.CallBack(unityTips, () =>
                {
                    ChangeKey((result) =>
                    {
                        if (!result) return;

                        CreateKeyEnumClass();

                        UIRedDotKeyEditorHelper.ChangeKeyByLink(m_UIRedDotModule.m_RedDotConfigAsset, linkData,
                            m_ChangeKeyData);

                        YIUIAutoTool.CloseWindowRefresh();
                    }, false);
                });
            }
            else
            {
                ChangeKey((result) =>
                {
                    if (!result) return;
                    CreateKeyEnumClass();
                });
            }
        }

        private void ChangeKey(Action<bool> resultAction = null, bool showTips = true)
        {
            if (m_ChangeKeyData == null)
            {
                UnityTipsHelper.Show($"请先 查询>>设置  需要修改的数据");
                return;
            }

            var (result, tips) = m_RedDotKeyAsset.ChangeKey(m_GetKeyData.Id, m_ChangeKeyData);

            if (showTips)
                UnityTipsHelper.Show(tips);

            resultAction?.Invoke(result);

            if (result)
            {
                m_GetKeyData    = null;
                m_ChangeKeyData = null;
            }
        }

        private void UpdateChangeData(RedDotKeyData getData)
        {
            if (getData == null)
            {
                m_ChangeKeyData = null;
                return;
            }

            m_ChangeKeyData = new RedDotKeyData(getData.Id, getData.Des);
        }

        #endregion

        #region 查

        [ShowInInspector]
        [HideIf("m_ERedDotKeyViewIndexType", ERedDotKeyViewIndexType.Add)]
        [LabelText("查询的ID")]
        [PropertyOrder(1001)]
        [InlineButton("GetKey", "查询")]
        [BoxGroup("查询数据", centerLabel: true)]
        [GUIColor(1, 1, 0)]
        private int m_GetId = 1;

        [ShowInInspector]
        [ReadOnly]
        [ShowIf("ShowGetKeyData")]
        [BoxGroup("查询数据", centerLabel: true)]
        [PropertyOrder(1002)]
        private RedDotKeyData m_GetKeyData;

        private bool ShowGetKeyData()
        {
            if (m_ERedDotKeyViewIndexType == ERedDotKeyViewIndexType.Add)
            {
                return false;
            }

            return m_GetKeyData != null;
        }

        private void GetKey()
        {
            var (data, tips) = m_RedDotKeyAsset.GetKey(m_GetId);
            m_GetKeyData     = data;
            UnityTipsHelper.Show(tips);
            UpdateChangeData(data);
        }

        #endregion

        #endregion

        [HideLabel]
        private enum ERedDotKeyViewIndexType
        {
            [LabelText("增")]
            Add = 1,

            [LabelText("删")]
            Delete = 2,

            [LabelText("改")]
            Change = 3,

            [LabelText("查")]
            Get = 4,
        }
    }
}
#endif