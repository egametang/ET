using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        /// <summary>
        /// 获取一个层级对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="panelLayer"></param>
        /// <returns></returns>
        public static RectTransform GetLayerRect(this YIUIMgrComponent self, EPanelLayer panelLayer)
        {
            self.m_AllPanelLayer.TryGetValue(panelLayer, out var rectDic);
            if (rectDic == null)
            {
                Debug.LogError($"没有这个层级 请检查 {panelLayer}");
                return null;
            }

            //只能有一个所以返回第一个
            foreach (var rect in rectDic.Keys)
            {
                return rect;
            }

            return null;
        }
    }
}