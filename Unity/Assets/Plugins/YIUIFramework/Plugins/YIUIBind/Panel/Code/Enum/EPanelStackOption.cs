using Sirenix.OdinInspector;

namespace YIUIFramework
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    /// <summary>
    /// Panel堆栈操作
    /// </summary>
    [LabelText("堆栈操作")]
    public enum EPanelStackOption
    {
        [LabelText("不操作 就叠加管理")]
        None = 0,

        [LabelText("显隐 不会触发动画")]
        Visible = 1,

        [LabelText("显隐 会触发动画")]
        VisibleTween = 2, //堆栈逻辑中没有关闭逻辑都是隐藏 一个会触发动画一个不会 这个会触发关闭动画

        [LabelText("省略 忽略 排除")]
        Omit = 3, //当他打开其他界面时 自己会被关闭 且不进入堆栈 自己被关闭时 会触发堆栈
    }
}