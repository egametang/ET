using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// 文本数据修改 基类
    /// </summary>
    public abstract class UIDataBindTextBase : UIDataBindSelectBase
    {
        [SerializeField]
        [Delayed] //延迟序列化
        [TextArea(2, 10)]
        [LabelText("字符串填充{0}{1}形式")]
        protected string m_Format;

        [SerializeField]
        [LabelText("可修改Enabled")]
        protected bool m_ChangeEnabled = true;

        [SerializeField]
        [LabelText("数字精度")]
        protected bool m_NumberPrecision = false;

        [SerializeField]
        [LabelText("精度值")]
        [ShowIf("m_NumberPrecision")]
        protected string m_NumberPrecisionStr = "F1";

        protected override int Mask()
        {
            return -1; //允许任何数据  只要你吧tostring写清楚就行
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            OnInit();
        }

        private void BaseSetEnabled(bool set)
        {
            if (!m_ChangeEnabled) return;
            SetEnabled(set);
        }

        private object[] m_ParamList;

        /// <summary>
        /// 当有format时 多参数就会生效依次填充
        /// 否则只会使用第一个值
        /// </summary>
        protected override void OnValueChanged()
        {
            if (!ExistText()) return;

            if (DataSelectDic == null || DataSelectDic.Count <= 0)
            {
                BaseSetEnabled(false);
                SetText("");
                return;
            }

            if (string.IsNullOrEmpty(m_Format))
            {
                var data  = DataSelectDic.First().Value;
                var value = GetDataToString(data);
                BaseSetEnabled(!string.IsNullOrEmpty(value));
                SetText(value);
            }
            else
            {
                if (m_ParamList == null || m_ParamList.Length != DataSelectDic.Count)
                {
                    m_ParamList = new object[DataSelectDic.Count];
                }

                var index = -1;
                foreach (var dataSelect in DataSelectDic.Values)
                {
                    index++;
                    m_ParamList[index] = GetDataToString(dataSelect);
                }

                BaseSetEnabled(true);

                try
                {
                    SetText(string.Format(m_Format, m_ParamList));
                }
                catch (FormatException exp)
                {
                    Logger.LogError($"{name} 字符串拼接Format 出错请检查是否有拼写错误  {m_Format}");
                    Logger.LogError(exp.Message, this);
                }
            }
        }

        private string GetDataToString(UIDataSelect dataSelect)
        {
            var dataValue = dataSelect?.Data?.DataValue;
            if (dataValue == null) return "";

            //如果不想用现在数据重写的tostring 可以在本类自行额外解析法
            if (!m_NumberPrecision) return dataValue.GetValueToString();

            switch (dataValue.UIBindDataType)
            {
                case EUIBindDataType.Float:
                    return dataValue.GetValue<float>().ToString(m_NumberPrecisionStr);
                case EUIBindDataType.Double:
                    return dataValue.GetValue<double>().ToString(m_NumberPrecisionStr);
                default:
                    return dataValue.GetValueToString();
            }
        }

        protected abstract void OnInit();
        protected abstract void SetEnabled(bool value);
        protected abstract void SetText(string  value);
        protected abstract bool ExistText();
    }
}