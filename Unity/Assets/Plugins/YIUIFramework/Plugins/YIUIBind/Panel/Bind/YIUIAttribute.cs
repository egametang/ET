using System;
using ET;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 用于标记
    /// ET组件的关系 >> 关联 >> UIBase
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class YIUIAttribute: BaseAttribute
    {
        //组件类型
        public EUICodeType YIUICodeType { get; }

        //层级类型 如果不是panel则无效
        public EPanelLayer YIUIPanelLayer { get; }

        public YIUIAttribute(EUICodeType codeType, EPanelLayer panelLayer = EPanelLayer.Any)
        {
            YIUICodeType   = codeType;
            YIUIPanelLayer = panelLayer;
        }
    }
}