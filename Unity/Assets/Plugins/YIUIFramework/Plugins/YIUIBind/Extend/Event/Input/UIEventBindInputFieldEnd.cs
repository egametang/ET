using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [InfoBox("提示: 可用事件参数 <参数1:string(输入的值)>")]
    [LabelText("输入栏<string> 结束时")]
    [RequireComponent(typeof(InputField))]
    [AddComponentMenu("YIUIBind/Event/输入栏 【InputFieldEnd】 UIEventBindInputFieldEnd")]
    public class UIEventBindInputFieldEnd : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("输入栏")]
        private InputField m_InputField;

        protected override bool IsTaskEvent => false;
        
        [NonSerialized]
        private List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType>
        {
            EUIEventParamType.String
        };

        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_InputField ??= GetComponent<InputField>();
        }

        private void OnEnable()
        {
            m_InputField.onEndEdit.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            m_InputField.onEndEdit.RemoveListener(OnValueChanged);
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