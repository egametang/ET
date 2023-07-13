using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Tween
{
	/// <summary>
	/// 补间动画系统
	/// </summary>
	public static class UniTween
	{
		private static bool _isInitialize = false;
		private static GameObject _driver = null;
		private static readonly List<TweenHandle> _tweens = new List<TweenHandle>(1000);
		private static readonly List<TweenHandle> _newer = new List<TweenHandle>(1000);
		private static readonly List<TweenHandle> _remover = new List<TweenHandle>(1000);

		/// <summary>
		/// 是否忽略时间戳缩放
		/// </summary>
		public static bool IgnoreTimeScale { set; get; } = false;

		/// <summary>
		/// 所有补间动画的播放速度
		/// </summary>
		public static float PlaySpeed { set; get; } = 1f;


		/// <summary>
		/// 初始化补间动画系统
		/// </summary>
		public static void Initalize()
		{
			if (_isInitialize)
				throw new Exception($"{nameof(UniTween)} is initialized !");

			if(_isInitialize == false)
			{
				// 创建驱动器
				_isInitialize = true;
				_driver = new GameObject($"[{nameof(UniTween)}]");
				_driver.AddComponent<UniTweenDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				UniLogger.Log($"{nameof(UniTween)} initalize !");
			}
		}

		/// <summary>
		/// 销毁补间动画系统
		/// </summary>
		public static void Destroy()
		{
			if (_isInitialize)
			{
				foreach (var tween in _tweens)
				{
					tween.Dispose();
				}
				foreach (var tween in _newer)
				{
					tween.Dispose();
				}
				foreach (var tween in _remover)
				{
					tween.Dispose();
				}

				_tweens.Clear();
				_newer.Clear();
				_remover.Clear();

				_isInitialize = false;
				if (_driver != null)
					GameObject.Destroy(_driver);
				UniLogger.Log($"{nameof(UniTween)} destroy all !");
			}
		}

		/// <summary>
		/// 更新补间动画系统
		/// </summary>
		internal static void Update()
		{
			if (_isInitialize)
			{
				// 添加新的补间动画
				if (_newer.Count > 0)
				{
					_tweens.AddRange(_newer);
					_newer.Clear();
				}

				// 更新所有补间动画			
				float deltaTime = IgnoreTimeScale ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
				deltaTime *= PlaySpeed;
				for (int i = 0; i < _tweens.Count; i++)
				{
					var handle = _tweens[i];
					if (handle.IsCanRemove())
						_remover.Add(handle);
					else
						handle.Update(deltaTime);
				}

				// 移除完成的补间动画
				for (int i = 0; i < _remover.Count; i++)
				{
					var handle = _remover[i];
					handle.Dispose();
					_tweens.Remove(handle);
				}
				_remover.Clear();
			}
		}


		/// <summary>
		/// 播放一个补间动画
		/// </summary>
		/// <param name="tweenRoot">补间根节点</param>
		/// <param name="unityObject">游戏对象</param>
		public static TweenHandle Play(ITweenNode tweenRoot, UnityEngine.Object unityObject = null)
		{
			DebugCheckInitialize();

			if (tweenRoot == null)
				throw new System.ArgumentNullException();

			TweenHandle handle = new TweenHandle(tweenRoot, unityObject);
			_newer.Add(handle);
			return handle;
		}

		/// <summary>
		/// 播放一个补间动画
		/// </summary>
		/// <param name="tweenChain">补间链节点</param>
		/// <param name="unityObject">游戏对象</param>
		public static TweenHandle Play(ITweenChain tweenChain, UnityEngine.Object unityObject = null)
		{
			ITweenNode tweenRoot = tweenChain as ITweenNode;
			if (tweenRoot == null)
				throw new System.InvalidCastException();

			return Play(tweenRoot, unityObject);
		}

		/// <summary>
		/// 播放一个补间动画
		/// </summary>
		/// <param name="chainNode">补间链节点</param>
		/// <param name="unityObject">游戏对象</param>
		public static TweenHandle Play(ChainNode chainNode, UnityEngine.Object unityObject = null)
		{
			ITweenNode tweenRoot = chainNode as ITweenNode;
			if (tweenRoot == null)
				throw new System.InvalidCastException();

			return Play(tweenRoot, unityObject);
		}

		/// <summary>
		/// 中途关闭补间动画
		/// </summary>
		/// <param name="tweenHandle">关闭的补间动画句柄</param>
		public static void Abort(TweenHandle tweenHandle)
		{
			DebugCheckInitialize();

			if (tweenHandle != null)
				tweenHandle.Abort();
		}

		/// <summary>
		/// 中途关闭补间动画
		/// </summary>
		/// <param name="unityObject">关闭该游戏对象下的所有补间动画</param>
		public static void Abort(UnityEngine.Object unityObject)
		{
			DebugCheckInitialize();

			int instanceID = unityObject.GetInstanceID();
			for (int i = 0; i < _tweens.Count; i++)
			{
				var handle = _tweens[i];
				if (handle.InstanceID == instanceID)
				{
					handle.Abort();
				}
			}
			for (int i = 0; i < _newer.Count; i++)
			{
				var handle = _newer[i];
				if (handle.InstanceID == instanceID)
				{
					handle.Abort();
				}
			}
		}


		#region Tween Allocate
		/// <summary>
		/// 执行节点
		/// </summary>
		/// <param name="execute">执行方法</param>
		public static ExecuteNode AllocateExecute(System.Action execute)
		{
			ExecuteNode node = new ExecuteNode();
			node.SetExecute(execute);
			return node;
		}

		/// <summary>
		/// 条件等待节点
		/// </summary>
		/// <param name="condition">条件方法</param>
		public static UntilNode AllocateUntil(System.Func<bool> condition)
		{
			UntilNode node = new UntilNode();
			node.SetCondition(condition);
			return node;
		}


		/// <summary>
		/// 并行执行的复合节点
		/// </summary>
		/// <param name="nodes">成员节点列表</param>
		public static ParallelNode AllocateParallel(params ITweenNode[] nodes)
		{
			ParallelNode node = new ParallelNode();
			node.AddNode(nodes);
			return node;
		}

		/// <summary>
		/// 顺序执行的复合节点
		/// </summary>
		/// <param name="nodes">成员节点列表</param>
		public static SequenceNode AllocateSequence(params ITweenNode[] nodes)
		{
			SequenceNode node = new SequenceNode();
			node.AddNode(nodes);
			return node;
		}

		/// <summary>
		/// 随机执行的复合节点
		/// </summary>
		/// <param name="nodes">成员节点列表</param>
		public static SelectorNode AllocateSelector(params ITweenNode[] nodes)
		{
			SelectorNode node = new SelectorNode();
			node.AddNode(nodes);
			return node;
		}


		/// <summary>
		/// 延迟计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="trigger">触发事件</param>
		public static TimerNode AllocateDelay(float delay, System.Action trigger = null)
		{
			UniTimer timer = UniTimer.CreateOnceTimer(delay);
			TimerNode node = new TimerNode(timer);
			node.SetTrigger(trigger);
			return node;
		}

		/// <summary>
		/// 重复计时节点
		/// 注意：该节点为无限时长
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="trigger">触发事件</param>
		public static TimerNode AllocateRepeat(float delay, float interval, System.Action trigger = null)
		{
			UniTimer timer = UniTimer.CreatePepeatTimer(delay, interval);
			TimerNode node = new TimerNode(timer);
			node.SetTrigger(trigger);
			return node;
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="duration">持续时间</param>
		/// <param name="trigger">触发事件</param>
		public static TimerNode AllocateRepeat(float delay, float interval, float duration, System.Action trigger = null)
		{
			UniTimer timer = UniTimer.CreatePepeatTimer(delay, interval, duration);
			TimerNode node = new TimerNode(timer);
			node.SetTrigger(trigger);
			return node;
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="maxRepeatCount">最大触发次数</param>
		/// <param name="trigger">触发事件</param>
		public static TimerNode AllocateRepeat(float delay, float interval, long maxRepeatCount, System.Action trigger = null)
		{
			UniTimer timer = UniTimer.CreatePepeatTimer(delay, interval, maxRepeatCount);
			TimerNode node = new TimerNode(timer);
			node.SetTrigger(trigger);
			return node;
		}

		/// <summary>
		/// 持续计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="duration">持续时间</param>
		/// <param name="trigger">触发事件</param>
		public static TimerNode AllocateDuration(float delay, float duration, System.Action trigger = null)
		{
			UniTimer timer = UniTimer.CreateDurationTimer(delay, duration);
			TimerNode node = new TimerNode(timer);
			node.SetTrigger(trigger);
			return node;
		}
		#endregion

		#region 调试方法
		[Conditional("DEBUG")]
		private static void DebugCheckInitialize()
		{
			if (_isInitialize == false)
				throw new Exception($"{nameof(UniTween)} not initialize !");
		}
		#endregion
	}
}