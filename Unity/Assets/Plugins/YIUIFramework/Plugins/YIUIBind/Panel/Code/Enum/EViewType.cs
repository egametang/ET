using Sirenix.OdinInspector;

namespace YIUIFramework
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    [LabelText("层级类型")]
    public enum EViewWindowType
    {
        [LabelText("窗口")]
        View = 0,

        [LabelText("弹窗")]
        Popup = 1,
    }
}