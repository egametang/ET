using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [InfoBox("提示: 可用事件参数 <参数1:int(下拉菜单被选择的索引值)>")]
    [LabelText("下拉菜单<int>")]
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("YIUIBind/Event/下拉菜单 【Dropdown】 UIEventBindDropdown")]
    public class UIEventBindDropdown : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("下拉菜单")]
        private Dropdown m_Dropdown;

        protected override bool IsTaskEvent => false;
        
        [NonSerialized]
        private List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType>
        {
            EUIEventParamType.Int
        };

        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_Dropdown ??= GetComponent<Dropdown>();
        }

        private void OnEnable()
        {
            m_Dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            m_Dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(int value)
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