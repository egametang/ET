#if UNITY_2020_3_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 分屏控件
    /// </summary>
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits>
        {
        }

        /// <summary>
        /// 窗口分屏适配
        /// </summary>
        public static void Adjuster(VisualElement root)
        {
            var topGroup = root.Q<VisualElement>("TopGroup");
            var bottomGroup = root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100f;
            bottomGroup.style.minHeight = 100f;
            root.Remove(topGroup);
            root.Remove(bottomGroup);
            var spliteView = new SplitView();
            spliteView.fixedPaneInitialDimension = 300;
            spliteView.orientation = TwoPaneSplitViewOrientation.Vertical;
            spliteView.contentContainer.Add(topGroup);
            spliteView.contentContainer.Add(bottomGroup);
            root.Add(spliteView);
        }
    }
}
#endif