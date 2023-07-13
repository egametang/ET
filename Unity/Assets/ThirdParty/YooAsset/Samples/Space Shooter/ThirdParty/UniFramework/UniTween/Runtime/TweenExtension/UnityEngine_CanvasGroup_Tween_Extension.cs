using UniFramework.Tween;

namespace UnityEngine
{
    public static class UnityEngine_CanvasGroup_Tween_Extension
    {
        public static FloatTween TweenAlpha(this CanvasGroup obj, float duration, float from, float to)
        {
            FloatTween node = FloatTween.Allocate(duration, from, to);
            node.SetOnUpdate((result) => { obj.alpha = result; });
            return node;
        }
        public static FloatTween TweenAlphaTo(this CanvasGroup obj, float duration, float to)
		{
            return TweenAlpha(obj, duration, obj.alpha, to);
		}
        public static FloatTween TweenAlphaFrom(this CanvasGroup obj, float duration, float from)
		{
            return TweenAlpha(obj, duration, from, obj.alpha);
        }
    }
}
