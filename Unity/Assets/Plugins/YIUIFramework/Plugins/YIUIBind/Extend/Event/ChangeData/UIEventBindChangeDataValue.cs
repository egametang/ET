using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// 点击事件绑定
    /// 与按钮无关
    /// 只要是任何可以被射线检测的物体都可以响应点击事件
    /// </summary>
    [InfoBox("提示: 可用事件参数 0个")]
    [LabelText("数据改变<null>")]
    [AddComponentMenu("YIUIBind/Event/数据改变 【ChangeDataValue】 UIEventBindChangeDataValue")]
    [RequireComponent(typeof (UIDataBindChange))]
    public class UIEventBindChangeDataValue: UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        private UIDataBindChange m_UIDataBindChange;
        protected override bool IsTaskEvent => false;
        [NonSerialized]
        private            List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType> { };
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Start()
        {
            InitBindChange();
        }

        private void InitBindChange()
        {
            m_UIDataBindChange ??= GetComponent<UIDataBindChange>();
            if (m_UIDataBindChange != null)
            {
                m_UIDataBindChange.AddChangeAction(OnChangeDataValue);
            }
        }

        private void OnChangeDataValue()
        {
            try
            {
                m_UIEvent?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        private new void OnDestroy()
        {
            if (m_UIDataBindChange != null)
            {
                m_UIDataBindChange.RemoveChangeAction(OnChangeDataValue);
            }
        }
    }
}