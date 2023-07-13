
namespace UniFramework.Tween
{
	/// <summary>
	/// 执行节点
	/// </summary>
	public sealed class ExecuteNode : ITweenNode
	{
		private System.Action _execute = null;

		/// <summary>
		/// 节点状态
		/// </summary>
		public ETweenStatus Status { private set; get; } = ETweenStatus.Idle;


		/// <summary>
		/// 设置执行方法
		/// </summary>
		public ExecuteNode SetExecute(System.Action execute)
		{
			_execute = execute;
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
				_execute?.Invoke();
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