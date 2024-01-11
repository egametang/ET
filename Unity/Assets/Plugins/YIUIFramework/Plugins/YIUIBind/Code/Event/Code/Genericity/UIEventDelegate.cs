namespace YIUIFramework
{
    //应该不用扩展了 5个参数你都不够用嘛
    //如果有请包起来用参数类sy
    //或使用ParamVo
    public delegate void UIEventDelegate();

    public delegate void UIEventDelegate<in P1>(P1 p1);

    public delegate void UIEventDelegate<in P1, in P2>(P1 p1, P2 p2);

    public delegate void UIEventDelegate<in P1, in P2, in P3>(P1 p1, P2 p2, P3 p3);

    public delegate void UIEventDelegate<in P1, in P2, in P3, in P4>(P1 p1, P2 p2, P3 p3, P4 p4);

    public delegate void UIEventDelegate<in P1, in P2, in P3, in P4, in P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
}