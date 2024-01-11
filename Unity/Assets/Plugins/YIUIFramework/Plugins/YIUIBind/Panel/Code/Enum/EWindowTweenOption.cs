using System;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    [LabelText("窗口选项")]
    [Flags]
    public enum EWindowOption
    {
        [LabelText("无")]
        None = 0,

        /// 当打开的参数不一致时
        /// 是否可以使用基础Open
        /// 如果允许 别人调用open了一个不存在的Open参数时
        /// 也可以使用默认的open打开界面 则你可以改为true
        [LabelText("字符串打开>参数不一致时 是否可以使用基础Open")]
        CanUseBaseOpen = 1,

        [LabelText("禁止使用ParamOpen")]
        BanParamOpen = 1 << 1, //paramOpen 是obj参数形式的Open

        [LabelText("我有其他IOpen时 允许用open")]
        HaveIOpenAllowOpen = 1 << 2, //默认情况下如果你有IOpen接口 就必须强制使用这个接口打开

        [LabelText("先开")]
        FitstOpen = 1 << 3, // 当前UI先播放动画然后关闭其他UI  否则 其他UI先关闭 当前UI后打开

        [LabelText("禁止动画")]
        BanTween = 1 << 10, //所有开关动画都会被跳过

        [LabelText("打开动画不可重复播放")]
        BanRepetitionOpenTween = 1 << 11, //生命周期内 打开动画只可以播放一次

        [LabelText("关闭动画不可重复播放")]
        BanRepetitionCloseTween = 1 << 12, //生命周期内 关闭动画只可以播放一次

        [LabelText("不等待打开动画")]
        BanAwaitOpenTween = 1 << 13, //这个影响的是打开动画之后的回调时机

        [LabelText("不等待关闭动画")]
        BanAwaitCloseTween = 1 << 14, //要动画完毕过后才会收到close回调 如果跳过就会立马收到

        [LabelText("我关闭时跳过其他的打开动画")]
        SkipOtherOpenTween = 1 << 15, //我关闭时 其他的界面直接打开不需要播放动画 

        [LabelText("我打开时跳过其他的关闭动画")]
        SkipOtherCloseTween = 1 << 16, //我打开时 其他的界面直接关闭不需要播放动画

        [LabelText("Home时 跳过我自己的打开动画")]
        SkipHomeOpenTween = 1 << 17, //被Home的界面直接打开 不播放动画
        
        [LabelText("播放动画时 可以操作")]
        AllowOptionByTween = 1 << 18, //默认播放动画的时候是不能操作UI的 不然容易出问题
    }

    public static class WindowOptionOptionExt
    {
        public static void Set(ref this EWindowOption owner, EWindowOption option)
        {
            owner |= option;
        }

        public static void Unset(ref this EWindowOption owner, EWindowOption option)
        {
            owner &= (~option);
        }
    }
}