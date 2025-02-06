#if UNITY_2019_4_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public static class UIElementsTools
    {
        public static void SetElementVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
#endif