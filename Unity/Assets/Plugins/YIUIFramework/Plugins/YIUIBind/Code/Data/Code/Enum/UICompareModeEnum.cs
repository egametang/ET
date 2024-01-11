using Sirenix.OdinInspector;

namespace YIUIFramework
{
    /// <summary>
    /// 比较运算枚举y
    /// </summary>
    [LabelText("比较运算")]
    public enum UICompareModeEnum
    {
        [LabelText("< 小于")]
        Less,

        [LabelText("≤ 小于等于")]
        LessEqual,

        [LabelText("= 等于")]
        Equal, //取反就是不等于

        [LabelText("> 大于")]
        Great,

        [LabelText("≥ 大于等于")]
        GreatEqual,
    }
}