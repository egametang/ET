using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 添加UIData前的准备数据
    /// 过度用@lsy
    /// </summary>
    public sealed class UINewData
    {
        [LabelText("名称")]
        [Delayed]
        public string Name;

        [HideLabel]
        public UIDataValue Data;
    }

    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    public sealed partial class UIData
    {
        [LabelText("名称")]
        [SerializeField]
        [ReadOnly]
        #if UNITY_EDITOR
        [InfoBox("此数据没有任何关联", InfoMessageType.Error, "ShowIfBindsTips")]
        #endif
        private string m_Name;

        /// <summary>
        /// 当前变量名称
        /// </summary>
        public string Name => m_Name;

        [SerializeField]
        [ReadOnly]
        [LabelText("唯一ID")]
        [HideInInspector]
        private int m_DataGuid;

        public int DataGuid => m_DataGuid;

        [OdinSerialize]
        private UIDataValue m_DataValue;

        public UIDataValue DataValue => m_DataValue;

        private UIData()
        {
        }

        public UIData(string name, UIDataValue dataValue)
        {
            m_Name      = name;
            m_DataValue = dataValue;
            m_DataGuid  = Guid.NewGuid().GetHashCode();
        }
    }
}