//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    //[DetailedInfoBox("UI 数据表 点击展开详细介绍", @"李胜扬")]
    [LabelText("UI 数据表")]
    [Serializable]
    [AddComponentMenu("YIUIBind/★★★UI Data Table 数据表★★★")]
    public sealed partial class UIBindDataTable : SerializedMonoBehaviour
    {
        [OdinSerialize]
        [HideLabel]
        [ShowInInspector]
        [Title("所有数据", TitleAlignment = TitleAlignments.Centered)]
        [DictionaryDrawerSettings(KeyLabel = "数据名称", ValueLabel = "数据内容", IsReadOnly = true,
            DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private Dictionary<string, UIData> m_DataDic = new Dictionary<string, UIData>();

        public IReadOnlyDictionary<string, UIData> DataDic => m_DataDic;

        private void Awake()
        {
            InitDataTable();
        }

        public UIData FindData(string dataName)
        {
            if (string.IsNullOrEmpty(dataName)) return null;

            return m_DataDic.TryGetValue(dataName, out var data) ? data : null;
        }

        public T FindDataValue<T>(string dataName) where T : UIDataValue
        {
            var uiData = FindData(dataName);
            if (uiData == null)
            {
                Logger.LogErrorContext(this, $"{name} 未找到这个数据请检查 {dataName}");
                return default;
            }

            if (uiData.DataValue == null)
            {
                Logger.LogErrorContext(this, $"{name} 数据没有初始化没有值 {dataName}");
                return default;
            }

            return (T)uiData.DataValue;
        }

        #region 递归初始化所有绑定数据

        private void InitDataTable()
        {
            InitializeBinds(transform);
        }

        private static void InitializeBinds(Transform transform)
        {
            #if YIUIMACRO_BIND_INITIALIZE
            Logger.LogErrorContext(transform,$"{transform.name} 初始化调用所有子类 UIDataBind 绑定");
            #endif
            var binds = ListPool<UIDataBind>.Get();
            transform.GetComponents(binds);
            foreach (var bind in binds)
            {
                bind.Initialize(true);
            }

            ListPool<UIDataBind>.Put(binds);

            foreach (Transform child in transform)
            {
                InitializeBindsDeep(child);
            }
        }

        private static void InitializeBindsDeep(Transform transform)
        {
            if (transform.HasComponent<UIBindDataTable>())
            {
                return;
            }

            var binds = ListPool<UIDataBind>.Get();
            transform.GetComponents(binds);
            foreach (var bind in binds)
            {
                bind.Initialize(true);
            }

            ListPool<UIDataBind>.Put(binds);

            foreach (Transform child in transform)
            {
                InitializeBindsDeep(child);
            }
        }

        #endregion
    }
}