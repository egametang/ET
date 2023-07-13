# UniFramework.Tween

一个轻量级的补间动画系统。（扩展方便，使用灵活，功能强大）

初始化补间动画系统

```c#
using UnityEngine;
using UniFramework.Tween;

void Start()
{
    // 初始化补间动画系统
    UniTween.Initalize();
}
```

传统编程

```c#
void Start()
{
    // 原地停留1秒，然后向上移动，停留1秒，然后同时缩小并回归原位。
    var tween = UniTween.AllocateSequence
    (
        UniTween.AllocateDelay(1f),
        this.transform.TweenMove(0.5f, new Vector3(0, 256, 0)),
        UniTween.AllocateDelay(1f),
        UniTween.AllocateParallel
        (
            this.transform.TweenScaleTo(0.5f, new Vector3(0.2f, 0.2f, 1f)),
            this.transform.TweenMove(0.5f, new Vector3(0, 0, 0))
        )
    );
    this.gameObject.PlayTween(tween); 
}
```

链式编程

```c#
void Start()
{
    // 原地停留1秒，然后向上移动，停留1秒，然后同时缩小并回归原位。
    ITweenChain tween = UniTween.AllocateSequence();
    tween.Delay(1f).
        Append(this.transform.TweenMove(0.5f, new Vector3(0, 256, 0))).
        Delay(1f).
        SwitchToParallel().
        Append(this.transform.TweenScaleTo(0.5f, new Vector3(0.2f, 0.2f, 1f))).
        Append(this.transform.TweenMove(0.5f, new Vector3(0, 0, 0)));
     this.gameObject.PlayTween(tween);  
}
```

默认的公共补间方法一共有30种，还可以使用AnimationCurve补充效果

```c#
public AnimationCurve EaseCurve;

public void PlayAnim()
{
    var tween = this.transform.TweenScaleTo(1f, Vector3.zero).SetEase(EaseCurve);
    UniTween.Play(tween);
}
```

扩展支持任意对象

```c#
// 扩展支持Image对象
public static class UnityEngine_UI_Image_Tween_Extension
{
    public static ColorTween TweenColor(this Image obj, float duration, Color from, Color to)
    {
        ColorTween node = ColorTween.Allocate(duration, from, to);
        node.SetOnUpdate((result) => { obj.color = result; });
        return node;
    }
    public static ColorTween TweenColorTo(this Image obj, float duration, Color to)
    {
        return TweenColor(obj, duration, obj.color, to);
    }
    public static ColorTween TweenColorFrom(this Image obj, float duration, Color from)
    {
        return TweenColor(obj, duration, from, obj.color);
    }
}
```

