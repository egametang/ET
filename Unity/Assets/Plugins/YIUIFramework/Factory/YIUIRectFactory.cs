using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 创建一个UI空对象时
    /// 不是UIBase 也不是entity
    /// 就是unity的一个obj而已
    /// </summary>
    public static class YIUIRectFactory
    {
        public static RectTransform CreateUIRect(RectTransform parent)
        {
            var obj  = new GameObject();
            var rect = obj.AddComponent<RectTransform>();
            if (parent != null)
                rect.SetParent(parent);
            return rect;
        }

        //重置为全屏自适应UI
        public static void ResetToFullScreen(this RectTransform self)
        {
            self.anchorMin          = Vector2.zero;
            self.anchorMax          = Vector2.one;
            self.anchoredPosition3D = Vector3.zero;
            self.pivot              = new Vector2(0.5f, 0.5f);
            self.offsetMax          = Vector2.zero;
            self.offsetMin          = Vector2.zero;
            self.sizeDelta          = Vector2.zero;
            self.localEulerAngles   = Vector3.zero;
            self.localScale         = Vector3.one;
        }

        //重置位置与旋转
        public static void ResetLocalPosAndRot(this RectTransform self)
        {
            self.localPosition = Vector3.zero;
            self.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 自动重置
        /// 一般情况下就2种 全屏的 那就全部归一
        /// 其他的 那就什么都不改 只修改大小就可以了
        /// </summary>
        public static void AutoReset(this RectTransform self)
        {
            if (self.anchorMax == Vector2.one && self.anchorMin == Vector2.zero)
            {
                self.ResetToFullScreen();
            }
            else
            {
                self.localScale = Vector3.one;
            }

            self.ResetLocalPosAndRot();
        }
    }
}