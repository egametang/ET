using Sirenix.OdinInspector;

namespace YIUIFramework
{
    /// <summary>
    /// 颜色叠加模式
    /// </summary>
    public enum ColorModeEnum
    {
        /// <summary>
        /// 混合源和覆盖。
        /// </summary>
        [LabelText("混合源和覆盖")]
        Blend = 0,

        /// <summary>
        /// 添加源颜色和叠加。
        /// </summary>
        [LabelText("添加源颜色和叠加")]
        Additive = 1,

        /// <summary>
        /// 在叠加之间筛选源颜色。
        /// </summary>
        [LabelText("在叠加之间筛选源颜色")]
        Screen = 2,
    }
}