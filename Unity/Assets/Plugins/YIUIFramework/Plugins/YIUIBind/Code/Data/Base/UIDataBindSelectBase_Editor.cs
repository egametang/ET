#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public abstract partial class UIDataBindSelectBase
    {
        [ValueDropdown("GetBindKeys")]
        [OnValueChanged("OnBindKeySelected")]
        [ShowInInspector]
        [PropertyOrder(-10)]
        [LabelText("选择绑定数据")]
        [NonSerialized]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private string m_SelectBindKey;

        [GUIColor(0, 1, 0)]
        [ButtonGroup("Select")]
        [Button("添加")]
        [PropertyOrder(-9)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void AddSelect()
        {
            if (string.IsNullOrEmpty(m_SelectBindKey))
            {
                var tips = $"请选择";
                UnityTipsHelper.Show(tips);
                Logger.LogError(tips);
                return;
            }

            if (m_DataSelectDic.Count >= SelectMax())
            {
                //不要问为什么不直接替换..
                UnityTipsHelper.ShowErrorContext(this, $"当前数据只允许最多选择 {SelectMax()} 个 已超过可选择上限 请先移除其他");
                m_SelectBindKey = "";
                return;
            }

            if (m_DataSelectDic.ContainsKey(m_SelectBindKey))
            {
                var tips = $"已存在 {m_SelectBindKey}";
                UnityTipsHelper.ShowError(tips);
                m_SelectBindKey = "";
                return;
            }

            var uiData = FindData(m_SelectBindKey);
            if (uiData == null)
            {
                var tips = $"没找到这个数据 {m_SelectBindKey}";
                UnityTipsHelper.ShowError(tips);
                m_SelectBindKey = "";
                return;
            }

            var data = new UIDataSelect(uiData);
            m_DataSelectDic.Add(m_SelectBindKey, data);
            m_SelectBindKey = "";
            base.OnValidate();
        }

        [GUIColor(1, 1, 0)]
        [ButtonGroup("Select")]
        [Button("移除")]
        [PropertyOrder(-8)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void RemoveSelect()
        {
            if (string.IsNullOrEmpty(m_SelectBindKey))
            {
                var tips1 = $"请选择";
                UnityTipsHelper.Show(tips1);
                Logger.LogError(tips1);
                return;
            }

            RemoveSelect(m_SelectBindKey);

            m_SelectBindKey = "";
        }

        private void RemoveSelect(string name)
        {
            if (!m_DataSelectDic.ContainsKey(name))
            {
                var tips = $"{name} 不存在无法移除";
                UnityTipsHelper.ShowError(tips);
                return;
            }

            UnbindData(name);
            m_DataSelectDic.Remove(name);
            OnValidate();
        }

        private bool IsValid(EUIBindDataType type)
        {
            return (1 << (int)type & Mask()) != 0;
        }

        //找到黑板字典里的所有数据
        private IEnumerable<string> GetBindKeys()
        {
            var allKey = new List<string>();

            if (DataTable == null)
            {
                Logger.LogError($"请检查未设置 数据表");
                return allKey;
            }

            //我现在有的 可能是错误的需要移除的
            foreach (var key in m_DataSelectDic.Keys)
            {
                if (DataTable.FindData(key) == null)
                {
                    allKey.Add($"[X]{key}");
                }
            }

            //表中可用的
            foreach (var data in DataTable.DataDic.Values)
            {
                if (IsValid(data.DataValue.UIBindDataType))
                {
                    if (string.IsNullOrEmpty(data.Name))
                    {
                        Logger.LogErrorContext(this, $"{name} 这个表中有null名称 请检查");
                        continue;
                    }

                    var ex = "     ";
                    foreach (var key in m_DataSelectDic.Keys)
                    {
                        if (key == data.Name)
                        {
                            ex = "[√]";
                            break;
                        }
                    }

                    ex = $"{ex}[{data.DataValue.UIBindDataType}] ";

                    allKey.Add($"{ex}{data.Name}");
                }
            }

            return allKey;
        }

        const string m_Pattern = @"\[[^\]]*\]|\s";

        //选择这个数据然后刷新
        private void OnBindKeySelected()
        {
            if (string.IsNullOrEmpty(m_SelectBindKey))
            {
                return;
            }

            m_SelectBindKey = System.Text.RegularExpressions.Regex.Replace(m_SelectBindKey, m_Pattern, "");
        }

        public override void RemoveBindData(UIData data)
        {
            RemoveSelect(data.Name);
        }

        /// <summary>
        /// 数据的名称的改变
        /// </summary>
        private void OnNameChanged(UIData change)
        {
            var keys = m_DataSelectDic.Keys.ToArray();

            foreach (var key in keys)
            {
                var data = m_DataSelectDic[key];
                if (change == data.Data) //换key
                {
                    m_DataSelectDic.Remove(key);            //移除老的
                    m_DataSelectDic.Add(change.Name, data); //添加新的
                }
            }

            OnValidate();
        }
    }
}
#endif