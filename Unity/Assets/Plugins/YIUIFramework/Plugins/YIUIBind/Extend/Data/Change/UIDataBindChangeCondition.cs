using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 改变绑定数据 配合 其他数据使用
    /// 由其他数据改变触发 然后吧对应的值修改
    /// 可挂载在任意对象上 需要一个载体
    /// </summary>
    [Serializable]
    [DetailedInfoBox("改变数据条件",
        @"当条件触发时 改变对应的数据 依赖UIDataBindChange
原理就是满足bool 条件后 触发改变多对多")]
    [LabelText("改变数据条件")]
    [RequireComponent(typeof(UIDataBindChange))]
    [AddComponentMenu("YIUIBind/Data/★改变数据条件 【Change Condition】 UIDataBindChangeCondition")]
    public class UIDataBindChangeCondition : UIDataBindBool
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        private UIDataBindChange m_UIDataBindChange;

        protected override void OnValueChanged()
        {
            var result = GetResult();
            if (!result) return;

            m_UIDataBindChange.ChangeDataValue();
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_UIDataBindChange ??= GetComponent<UIDataBindChange>();
        }
    }
}