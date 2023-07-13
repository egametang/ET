
namespace UniFramework.Tween
{
	/// <summary>
	/// 计时器节点
	/// </summary>
	public sealed class TimerNode : ITweenNode
	{
		private readonly UniTimer _timer;
		private System.Action _trigger;
		
		/// <summary>
		/// 节点状态
		/// </summary>
		public ETweenStatus Status { private set; get; } = ETweenStatus.Idle;


		public TimerNode(UniTimer timer)
		{
			_timer = timer;
		}

		/// <summary>
		/// 设置触发方法
		/// </summary>
		public TimerNode SetTrigger(System.Action trigger)
		{
			_trigger = trigger;
			return this;
		}

		void ITweenNode.OnUpdate(float deltaTime)
		{
			if (Status == ETweenStatus.Idle)
			{
				Status = ETweenStatus.Runing;
			}

			if (Status == ETweenStatus.Runing)
			{
				if (_timer.Update(deltaTime))
					_trigger?.Invoke();

				bool result = _timer.IsOver;
				if (result)
					Status = ETweenStatus.Completed;
			}
		}
		void ITweenNode.OnDispose()
		{
		}
		void ITweenNode.Abort()
		{
			Status = ETweenStatus.Abort;
		}
	}
}