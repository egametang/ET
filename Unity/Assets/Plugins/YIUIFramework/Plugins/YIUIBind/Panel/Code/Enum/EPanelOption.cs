using System;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    [LabelText("界面选项")]
    [Flags]
    public enum EPanelOption
    {
        [LabelText("无")]
        None = 0,

        [LabelText("容器类界面")]
        Container = 1, //比如伤害飘字 此类界面如果做到panel层会被特殊处理 建议还是不要放到panel层

        [LabelText("永久缓存界面")]
        ForeverCache = 1 << 1, //永远不会被摧毁 与禁止关闭不同这个可以关闭 只是不销毁 也可相当于无限长的倒计时

        [LabelText("倒计时缓存界面")]
        TimeCache = 1 << 2, //被关闭后X秒之后在摧毁 否则理解摧毁

        [LabelText("禁止关闭的界面")]
        DisClose = 1 << 3, //是需要一直存在的你可以隐藏 但是你不能摧毁

        [LabelText("忽略返回 返回操作会跳过这个界面")]
        IgnoreBack = 1 << 4, //他的打开与关闭不会触发返回功能 堆栈功能
    }

    public static class PanelOptionExt
    {
        public static void Set(ref this EPanelOption owner, EPanelOption option)
        {
            owner |= option;
        }

        public static void Unset(ref this EPanelOption owner, EPanelOption option)
        {
            owner &= (~option);
        }
    }
}