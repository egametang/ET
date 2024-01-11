using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [InfoBox("提示: 可用事件参数 <参数1:string(输入的值)>")]
    [LabelText("输入栏<string> 变化时")]
    [RequireComponent(typeof(TMP_InputField))]
    [AddComponentMenu("YIUIBind/Event/输入栏 【InputField TMP】 UIEventBindInputFieldTMP")]
    public class UIEventBindInputFieldTMP : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("输入栏")]
        private TMP_InputField m_InputField;

        protected override bool IsTaskEvent => false;
        
        [NonSerialized]
        private List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType>
        {
            EUIEventParamType.String
        };

        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_InputField ??= GetComponent<TMP_InputField>();
        }

        private void OnEnable()
        {
            m_InputField.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            m_InputField.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(string value)
        {
            try
            {
                m_UIEvent?.Invoke(value);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }
    }
}