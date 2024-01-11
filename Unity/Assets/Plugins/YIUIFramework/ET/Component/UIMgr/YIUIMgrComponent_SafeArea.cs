using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 当前使用  UnitysafeArea API 进行UI刘海屏自适应
    /// UnitysafeArea API 也不太靠谱 可以在这里扩展自己的东西
    /// </summary>
    public partial class YIUIMgrComponent
    {
        /// <summary>
        /// 在刘海屏机子时，是否打开黑边
        /// </summary>
        public const bool OpenBlackBorder = false;

        //启用2倍安全 则左右2边都会裁剪
        public const bool DoubleSafe = false;

        //安全区
        public static Rect g_SafeArea;

        /// <summary>
        /// 横屏设置时，界面左边离屏幕的距离
        /// </summary>
        public static float SafeAreaLeft => Screen.orientation == ScreenOrientation.LandscapeRight
                ? Screen.width - g_SafeArea.xMax
                : g_SafeArea.x;

        internal static ScreenOrientation ScreenOrientation = Screen.orientation;

        internal void InitSafeArea()
        {
            var safeAreaX = Math.Max(Screen.safeArea.x, Screen.width - Screen.safeArea.xMax);
            var safeAreaY = Math.Max(Screen.safeArea.y, Screen.height - Screen.safeArea.yMax);

            #if UNITY_EDITOR
            //这里可调试 
            //safeAreaX = 100;
            //safeAreaY = 100;
            #endif

            g_SafeArea = new Rect(safeAreaX,
                safeAreaY,
                DesignScreenWidth_F - GetSafeValue(safeAreaX),
                DesignScreenHeight_F - GetSafeValue(safeAreaY));

            this.InitUISafeArea();
        }

        private float GetSafeValue(float safeValue)
        {
            return DoubleSafe? safeValue * 2 : safeValue;
        }

        private void InitUISafeArea()
        {
            UILayerRoot.anchoredPosition = new Vector2(g_SafeArea.x, -g_SafeArea.y);
            if (DoubleSafe)
            {
                UILayerRoot.offsetMax = new Vector2(-g_SafeArea.x, UILayerRoot.offsetMax.y);
                UILayerRoot.offsetMin = new Vector2(UILayerRoot.offsetMin.x, g_SafeArea.y);
            }
            else
            {
                //TODO 单边时需要考虑手机是左还是右
                UILayerRoot.offsetMax = new Vector2(0, UILayerRoot.offsetMax.y);
                UILayerRoot.offsetMin = new Vector2(UILayerRoot.offsetMin.x, 0);
            }
        }
    }
}