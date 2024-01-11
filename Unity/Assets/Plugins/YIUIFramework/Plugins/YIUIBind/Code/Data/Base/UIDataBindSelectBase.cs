//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// 基类 都需要选择绑定数据时
    /// </summary>
    [Serializable]
    public abstract partial class UIDataBindSelectBase: UIDataBind
    {
        [OdinSerialize]
        [LabelText("所有已绑定数据")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "数据名称", ValueLabel = "数据内容", IsReadOnly = true,
            DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        [ReadOnly]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private Dictionary<string, UIDataSelect> m_DataSelectDic = new Dictionary<string, UIDataSelect>();

        public IReadOnlyDictionary<string, UIDataSelect> DataSelectDic => m_DataSelectDic;

        #region 快捷获取 方便子类操作

        protected T GetFirstValue<T>(T defaultValue = default)
        {
            if (DataSelectDic.Count <= 0) return defaultValue;

            var data = DataSelectDic?.First().Value?.Data;
            return data == null? defaultValue : data.GetValue(defaultValue);
        }

        protected void SetFirstValue<T>(T value, bool force = false)
        {
            if (DataSelectDic.Count <= 0) return;

            DataSelectDic?.First().Value?.Data?.Set(value, force);
        }

        [NonSerialized]
        [HideInEditorMode]
        private List<UIDataSelect> m_DataSelectList;

        private List<UIDataSelect> DataSelectList => m_DataSelectList ??= DataSelectDic.Values.ToList();
        
        protected T GetValue<T>(int index, T defaultValue = default)
        {
            if (DataSelectList.Count <= 0 || (index + 1) > DataSelectList.Count) return defaultValue;
            var data = DataSelectList[index]?.Data;
            return data == null? defaultValue : data.GetValue(defaultValue);
        }

        protected void SetValue<T>(int index, T value, bool force = false)
        {
            if (DataSelectList.Count <= 0 || (index + 1) > DataSelectList.Count) return;
            var data = DataSelectList[index]?.Data;
            data?.Set(value,force);
        }
        
        #endregion

        #region 绑定

        protected override void BindData()
        {
            foreach (var dataSelect in m_DataSelectDic)
            {
                var dataName = dataSelect.Key;

                if (string.IsNullOrEmpty(dataName))
                {
                    continue;
                }

                var data = FindData(dataName);
                if (data == null)
                {
                    continue;
                }

                data.AddValueChangeAction(OnValueChanged);
                #if UNITY_EDITOR
                data.OnDataChangAction += OnNameChanged;
                data.AddBind(this);
                #endif
                dataSelect.Value.RefreshData(data);
            }

            OnValueChanged();
        }

        private void UnbindData(string dataName)
        {
            if (m_DataSelectDic.TryGetValue(dataName, out var value))
            {
                UnbindData(value);
            }
        }

        private void UnbindData(UIDataSelect value)
        {
            var data = value?.Data;
            if (data == null)
            {
                Logger.LogErrorContext(this, $"{name}空数据 请检查为什么 当前是否不在预制件编辑器中使用了");
                return;
            }

            data.RemoveValueChangeAction(OnValueChanged);
            #if UNITY_EDITOR
            data.OnDataChangAction -= OnNameChanged;
            data.RemoveBind(this);
            #endif
        }

        //解除所有绑定
        protected override void UnBindData()
        {
            foreach (var dic in m_DataSelectDic)
            {
                UnbindData(dic.Value);
            }
        }

        #endregion

        /// <summary>
        /// 数据改变时
        /// </summary>
        protected abstract void OnValueChanged();

        //没有特别需要处理的 但是为了结构一致 所以继承的都要调用
        //要调用base.OnRefreshData(); 以免多重基础后的错误问题
        protected override void OnRefreshData()
        {
        }

        /// <summary>
        /// 可选择类型 用掩码
        /// </summary>
        /// <returns></returns>
        protected abstract int Mask();

        /// <summary>
        /// 可选择最大数量 默认最大
        /// </summary>
        /// <returns></returns>
        protected virtual int SelectMax()
        {
            return int.MaxValue;
        }
    }
}