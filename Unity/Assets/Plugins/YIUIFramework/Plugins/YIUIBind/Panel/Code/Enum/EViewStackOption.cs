using Sirenix.OdinInspector;

namespace YIUIFramework
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    /// <summary>
    /// View堆栈操作
    /// </summary>
    [LabelText("堆栈操作")]
    public enum EViewStackOption
    {
        [LabelText("不操作 就叠加管理")]
        None = 0,

        [LabelText("显隐 不触发动画")]
        Visible = 1, //直接显影

        [LabelText("显隐 会触发动画")]
        VisibleTween = 2, //堆栈逻辑中没有关闭逻辑都是隐藏 一个会触发动画一个不会 这个会触发关闭动画
    }
}