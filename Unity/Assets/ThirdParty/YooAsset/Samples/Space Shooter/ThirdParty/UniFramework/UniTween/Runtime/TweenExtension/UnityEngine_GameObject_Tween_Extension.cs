using UniFramework.Tween;

namespace UnityEngine
{
	public static class UnityEngine_GameObject_Tween_Extension
	{
		/// <summary>
		/// 播放补间动画
		/// </summary>
		public static TweenHandle PlayTween(this GameObject go, ITweenNode tweenRoot)
		{
			return UniTween.Play(tweenRoot, go);
		}

		/// <summary>
		/// 播放补间动画
		/// </summary>
		public static TweenHandle PlayTween(this GameObject go, ITweenChain tweenRoot)
		{
			return UniTween.Play(tweenRoot, go);
		}

		/// <summary>
		/// 播放补间动画
		/// </summary>
		public static TweenHandle PlayTween(this GameObject go, ChainNode tweenRoot)
		{
			return UniTween.Play(tweenRoot, go);
		}
	}
}
