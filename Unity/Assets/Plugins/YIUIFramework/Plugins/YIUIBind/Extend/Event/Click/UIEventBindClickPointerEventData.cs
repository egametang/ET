using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YIUIFramework
{
    /// <summary>
    /// 点击事件绑定
    /// 与按钮无关
    /// 只要是任何可以被射线检测的物体都可以响应点击事件
    /// </summary>
    [InfoBox("提示: 可用事件参数 1个 , Object(PointerEventData)")]
    [LabelText("点击<PointerEventData>")]
    [AddComponentMenu("YIUIBind/Event/点击 【ClickPointerEventData】 UIEventBindClickPointerEventData")]
    public class UIEventBindClickPointerEventData: UIEventBindClick
    {
        [NonSerialized]
        private List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType> { EUIEventParamType.Object, };

        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;
        
        protected override void OnUIEvent(PointerEventData eventData)
        {
            //额外添加 如果想要这个点击事件 使用此监听
            //响应方法那边参数是obj 自己在转一次 没有扩展这个参数 因为没必要
            m_UIEvent?.Invoke(eventData as object);
        }
    }
}