using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace YIUIFramework
{
    [Serializable]
    [HideReferenceObjectPicker]
    internal class UIDataChangeRef
    {
        //当前的变量
        [SerializeField]
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

        [ShowInInspector]
        [OdinSerialize]
        private UIDataValue m_ChangeData;

        public UIDataValue ChangeData => m_ChangeData;

        private UIDataChangeRef()
        {
        }

        public UIDataChangeRef(UIData data)
        {
            Refresh(data);
        }

        //刷新数据
        public void Refresh(UIData data)
        {
            m_Data     = data;
            m_DataName = m_Data.Name;

            if (m_ChangeData == null || m_ChangeData.UIBindDataType != data.DataValue.UIBindDataType)
            {
                m_ChangeData = UIDataHelper.GetNewDataValue(data.DataValue);
            }

            #if UNITY_EDITOR
            m_DataValue = m_Data.GetValueToString();
            #endif
        }
    }
}