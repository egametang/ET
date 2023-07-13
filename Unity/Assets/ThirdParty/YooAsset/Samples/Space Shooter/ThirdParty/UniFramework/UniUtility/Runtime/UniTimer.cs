
namespace UniFramework.Utility
{
	public sealed class UniTimer
	{
		/// <summary>
		/// 延迟后，触发一次
		/// </summary>
		public static UniTimer CreateOnceTimer(float delay)
		{
			return new UniTimer(delay, -1, -1, 1);
		}
		
		/// <summary>
		/// 延迟后，永久性的间隔触发
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		public static UniTimer CreatePepeatTimer(float delay, float interval)
		{
			return new UniTimer(delay, interval, -1, -1);
		}

		/// <summary>
		/// 延迟后，在一段时间内间隔触发
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="duration">触发周期</param>
		public static UniTimer CreatePepeatTimer(float delay, float interval, float duration)
		{
			return new UniTimer(delay, interval, duration, -1);
		}

		/// <summary>
		/// 延迟后，间隔触发一定次数
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="maxTriggerCount">最大触发次数</param>
		public static UniTimer CreatePepeatTimer(float delay, float interval, long maxTriggerCount)
		{
			return new UniTimer(delay, interval, -1, maxTriggerCount);
		}
		
		/// <summary>
		/// 延迟后，在一段时间内触发
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="duration">触发周期</param>
		public static UniTimer CreateDurationTimer(float delay, float duration)
		{
			return new UniTimer(delay, -1, duration, -1);
		}

		/// <summary>
		/// 延迟后，永久触发
		/// </summary>
		public static UniTimer CreateForeverTimer(float delay)
		{
			return new UniTimer(delay, -1, -1, -1);
		}
		

		private readonly float _intervalTime;
		private readonly float _durationTime;
		private readonly long _maxTriggerCount;

		// 需要重置的变量
		private float _delayTimer = 0;
		private float _durationTimer = 0;
		private float _intervalTimer = 0;
		private long _triggerCount = 0;

		/// <summary>
		/// 延迟时间
		/// </summary>
		public float DelayTime { private set; get; }

		/// <summary>
		/// 是否已经结束
		/// </summary>
		public bool IsOver { private set; get; }

		/// <summary>
		/// 是否已经暂停
		/// </summary>
		public bool IsPause { private set; get; }

		/// <summary>
		/// 延迟剩余时间
		/// </summary>
		public float Remaining
		{
			get
			{
				if (IsOver)
					return 0f;
				else
					return System.Math.Max(0f, DelayTime - _delayTimer);
			}
		}

		/// <summary>
		/// 计时器
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="duration">运行时间</param>
		/// <param name="maxTriggerTimes">最大触发次数</param>
		public UniTimer(float delay, float interval, float duration, long maxTriggerCount)
		{
			DelayTime = delay;
			_intervalTime = interval;
			_durationTime = duration;
			_maxTriggerCount = maxTriggerCount;
		}

		/// <summary>
		/// 暂停计时器
		/// </summary>
		public void Pause()
		{
			IsPause = true;
		}

		/// <summary>
		/// 恢复计时器
		/// </summary>
		public void Resume()
		{
			IsPause = false;
		}

		/// <summary>
		/// 结束计时器
		/// </summary>
		public void Kill()
		{
			IsOver = true;
		}

		/// <summary>
		/// 重置计时器
		/// </summary>
		public void Reset()
		{
			_delayTimer = 0;
			_durationTimer = 0;
			_intervalTimer = 0;
			_triggerCount = 0;
			IsOver = false;
			IsPause = false;
		}

		/// <summary>
		/// 更新计时器
		/// </summary>
		public bool Update(float deltaTime)
		{
			if (IsOver || IsPause)
				return false;

			_delayTimer += deltaTime;
			if (_delayTimer < DelayTime)
				return false;

			if(_intervalTime > 0)
				_intervalTimer += deltaTime;
			if (_durationTime > 0)
				_durationTimer += deltaTime;

			// 检测间隔执行
			if (_intervalTime > 0)
			{		
				if (_intervalTimer < _intervalTime)
					return false;
				_intervalTimer = 0;
			}

			// 检测结束条件
			if (_durationTime > 0)
			{
				if (_durationTimer >= _durationTime)
					Kill();
			}

			// 检测结束条件
			if (_maxTriggerCount > 0)
			{
				_triggerCount++;
				if (_triggerCount >= _maxTriggerCount)
					Kill();
			}

			return true;
		}
	}
}