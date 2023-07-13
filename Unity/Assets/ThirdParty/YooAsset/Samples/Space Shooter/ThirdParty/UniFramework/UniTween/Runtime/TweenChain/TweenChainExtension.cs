
namespace UniFramework.Tween
{
	public static partial class TweenChainExtension
	{
		/// <summary>
		/// 执行节点
		/// </summary>
		public static ITweenChain Execute(this ITweenChain chain, System.Action execute)
		{
			return chain.Append(UniTween.AllocateExecute(execute));
		}

		/// <summary>
		/// 条件等待节点
		/// </summary>
		public static ITweenChain Until(this ITweenChain chain, System.Func<bool> condition)
		{
			return chain.Append(UniTween.AllocateUntil(condition));
		}


		/// <summary>
		/// 并行执行的复合节点
		/// </summary>
		public static ITweenChain AppendParallel(this ITweenChain chain, params ITweenNode[] nodes)
		{
			var node = UniTween.AllocateParallel(nodes);
			chain.Append(node);
			return chain;
		}

		/// <summary>
		/// 顺序执行的复合节点
		/// </summary>
		public static ITweenChain AppendSequence(this ITweenChain chain, params ITweenNode[] nodes)
		{
			var node = UniTween.AllocateSequence(nodes);
			chain.Append(node);
			return chain;
		}

		/// <summary>
		/// 随机执行的复合节点
		/// </summary>
		public static ITweenChain AppendSelector(this ITweenChain chain, params ITweenNode[] nodes)
		{
			var node = UniTween.AllocateSelector(nodes);
			chain.Append(node);
			return chain;
		}

		
		/// <summary>
		/// 并行执行的复合节点
		/// </summary>
		public static ITweenChain SwitchToParallel(this ITweenChain chain, params ITweenNode[] nodes)
		{
			var node = UniTween.AllocateParallel(nodes);
			chain.Append(node);
			return node;
		}

		/// <summary>
		/// 顺序执行的复合节点
		/// </summary>
		public static ITweenChain SwitchToSequence(this ITweenChain chain, params ITweenNode[] nodes)
		{
			var node = UniTween.AllocateSequence(nodes);
			chain.Append(node);
			return node;
		}

		/// <summary>
		/// 随机执行的复合节点
		/// </summary>
		public static ITweenChain SwitchToSelector(this ITweenChain chain, params ITweenNode[] nodes)
		{
			var node = UniTween.AllocateSelector(nodes);
			chain.Append(node);
			return node;
		}


		/// <summary>
		/// 延迟计时节点
		/// </summary>
		public static ITweenChain Delay(this ITweenChain chain, float delay, System.Action trigger = null)
		{
			return chain.Append(UniTween.AllocateDelay(delay, trigger));
		}

		/// <summary>
		/// 重复计时节点
		/// 注意：该节点为无限时长
		/// </summary>
		public static ITweenChain Repeat(this ITweenChain chain, float delay, float interval, System.Action trigger = null)
		{
			return chain.Append(UniTween.AllocateRepeat(delay, interval, trigger));
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		public static ITweenChain Repeat(this ITweenChain chain, float delay, float interval, float duration, System.Action trigger = null)
		{
			return chain.Append(UniTween.AllocateRepeat(delay, interval, duration, trigger));
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		public static ITweenChain Repeat(this ITweenChain chain, float delay, float interval, long maxRepeatCount, System.Action trigger = null)
		{
			return chain.Append(UniTween.AllocateRepeat(delay, interval, maxRepeatCount, trigger));
		}

		/// <summary>
		/// 持续计时节点
		/// </summary>
		public static ITweenChain Duration(this ITweenChain chain, float delay, float duration, System.Action trigger = null)
		{
			return chain.Append(UniTween.AllocateDuration(delay, duration, trigger));
		}
	}
}