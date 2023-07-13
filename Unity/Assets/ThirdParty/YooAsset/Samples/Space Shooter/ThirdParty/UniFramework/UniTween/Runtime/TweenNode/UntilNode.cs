
namespace UniFramework.Tween
{
	/// <summary>
	/// 条件等待节点
	/// </summary>
	public sealed class UntilNode : ITweenNode
	{
		private System.Func<bool> _condition = null;

		/// <summary>
		/// 节点状态
		/// </summary>
		public ETweenStatus Status { private set; get; } = ETweenStatus.Idle;

		/// <summary>
		/// 设置条件方法
		/// </summary>
		public UntilNode SetCondition(System.Func<bool> condition)
		{
			_condition = condition;
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
				if (_condition == null)
				{
					UniLogger.Warning($"The {nameof(UntilNode)} condition is null. Set completed !");
					Status = ETweenStatus.Completed;
				}
				else
				{
					bool result = _condition.Invoke();
					if (result)
						Status = ETweenStatus.Completed;
				}
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