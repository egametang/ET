using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class UIDataBoolRef
    {
        //当前的变量
        [OdinSerialize]
        [HideInInspector]
        private UIData m_Data;

        public UIData Data => m_Data;

        [ShowInInspector]
        [LabelText("名称")]
        [ReadOnly]
        private string m_DataName;

        #if UNITY_EDITOR
        [ShowInInspector]
        [LabelText("值")]
        [ReadOnly]
        private string m_DataValue;
        #endif
        [SerializeField]
        #if UNITY_EDITOR
        [OnValueChanged("OnValueChangedCompareMode")]
        #endif
        [ShowIf("ShowCompareMode")]
        private UICompareModeEnum m_CompareMode = UICompareModeEnum.Equal;

        private bool ShowCompareMode()
        {
            if (m_Data.DataValue.UIBindDataType == EUIBindDataType.Int ||
                m_Data.DataValue.UIBindDataType == EUIBindDataType.Float)
            {
                return true;
            }

            return false;
        }

        [OdinSerialize]
        [HideLabel]
        [ShowInInspector]
        public UIDataValue m_ReferenceData;

        public UIDataValue ReferenceData => m_ReferenceData;

        [SerializeField]
        [LabelText("取反")]
        [ShowInInspector]
        private bool m_Reverse;

        private UIDataBoolRef()
        {
        }

        public UIDataBoolRef(UIData data)
        {
            Refresh(data);
        }

        //刷新数据
        public void Refresh(UIData data)
        {
            m_Data     = data;
            m_DataName = m_Data.Name;
            #if UNITY_EDITOR
            m_DataValue = m_Data.GetValueToString();
            #endif
            if (m_Data.DataValue.UIBindDataType == EUIBindDataType.Bool ||
                m_Data.DataValue.UIBindDataType == EUIBindDataType.String)
            {
                m_CompareMode = UICompareModeEnum.Equal;
            }

            if (m_ReferenceData == null || (m_ReferenceData.UIBindDataType != m_Data.DataValue.UIBindDataType))
            {
                m_ReferenceData = UIDataHelper.GetNewDataValue(m_Data.DataValue.UIBindDataType);
            }
        }

        #if UNITY_EDITOR

        //比较运算的修改
        private void OnValueChangedCompareMode()
        {
            if (m_Data.DataValue.UIBindDataType == EUIBindDataType.String && m_CompareMode != UICompareModeEnum.Equal)
            {
                m_CompareMode = UICompareModeEnum.Equal;
                UnityTipsHelper.ShowError($"字符串类型 只允许使用 == 运算判断 != 使用取反");
            }

            if (m_Data.DataValue.UIBindDataType == EUIBindDataType.Bool && m_CompareMode != UICompareModeEnum.Equal)
            {
                m_CompareMode = UICompareModeEnum.Equal;
                UnityTipsHelper.ShowError($"布尔类型 只允许使用 == 运算判断 != 使用取反");
            }
        }
        #endif

        /// <summary>
        /// 获取最终比较结果
        /// </summary>
        /// <returns></returns>
        public bool GetResult()
        {
            if (m_Data?.DataValue == null)
            {
                return false;
            }

            var result = false;

            switch (m_Data.DataValue.UIBindDataType)
            {
                case EUIBindDataType.Bool:
                    var valueBool     = m_Data.GetValue<bool>();
                    var referenceBool = m_ReferenceData.GetValue<bool>();
                    result = valueBool == referenceBool;
                    break;
                case EUIBindDataType.Int:
                    var valueInteger     = m_Data.GetValue<int>();
                    var referenceInteger = m_ReferenceData.GetValue<int>();
                    switch (m_CompareMode)
                    {
                        case UICompareModeEnum.Less:
                            result = valueInteger < referenceInteger;
                            break;
                        case UICompareModeEnum.LessEqual:
                            result = valueInteger <= referenceInteger;
                            break;
                        case UICompareModeEnum.Equal:
                            result = valueInteger == referenceInteger;
                            break;
                        case UICompareModeEnum.Great:
                            result = valueInteger > referenceInteger;
                            break;
                        case UICompareModeEnum.GreatEqual:
                            result = valueInteger >= referenceInteger;
                            break;
                        default:
                            Logger.LogError($"不可能有其他类型 不允许扩展 {m_CompareMode}");
                            break;
                    }

                    break;
                case EUIBindDataType.Float:
                    var valueFloat     = m_Data.GetValue<float>();
                    var referenceFloat = m_ReferenceData.GetValue<float>();
                    switch (m_CompareMode)
                    {
                        case UICompareModeEnum.Less:
                            result = valueFloat < referenceFloat;
                            break;
                        case UICompareModeEnum.LessEqual:
                            result = valueFloat <= referenceFloat;
                            break;
                        case UICompareModeEnum.Equal:
                            result = Mathf.Approximately(valueFloat, referenceFloat);
                            break;
                        case UICompareModeEnum.Great:
                            result = valueFloat > referenceFloat;
                            break;
                        case UICompareModeEnum.GreatEqual:
                            result = valueFloat >= referenceFloat;
                            break;
                        default:
                            Logger.LogError($"不可能有其他类型 不允许扩展 {m_CompareMode}");
                            break;
                    }

                    break;
                case EUIBindDataType.String:
                    var valueString     = m_Data.GetValue<string>();
                    var referenceString = m_ReferenceData.GetValue<string>();
                    switch (m_CompareMode)
                    {
                        case UICompareModeEnum.Equal:
                            result = valueString == referenceString;
                            break;
                        case UICompareModeEnum.Less:
                        case UICompareModeEnum.LessEqual:
                        case UICompareModeEnum.Great:
                        case UICompareModeEnum.GreatEqual:
                        default:
                            Logger.LogError($"String 的 bool 比较只支持 == 取反就是 != 不支持其他");
                            break;
                    }
                    
                    break;
                default:
                    Logger.LogError($"类型比较 不支持这个类型 {m_Data.DataValue.UIBindDataType}");
                    return false;
            }

            if (m_Reverse)
            {
                result = !result;
            }

            return result;
        }
    }
}