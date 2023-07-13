using System.Collections.Generic;

namespace UniFramework.Tween
{
	/// <summary>
	/// 复合节点基类
	/// </summary>
	public abstract class ChainNode : ITweenNode, ITweenChain
	{
		protected List<ITweenNode> _nodes = new List<ITweenNode>();

		private System.Action _onBegin = null;
		private System.Action _onComplete = null;
		private System.Action _onDispose = null;

		/// <summary>
		/// 节点状态
		/// </summary>
		public ETweenStatus Status { private set; get; } = ETweenStatus.Idle;


		/// <summary>
		/// 添加节点
		/// </summary>
		public void AddNode(ITweenNode node)
		{
			if (_nodes.Contains(node) == false)
				_nodes.Add(node);
		}

		/// <summary>
		/// 添加节点
		/// </summary>
		/// <param name="nodes"></param>
		public void AddNode(params ITweenNode[] nodes)
		{
			foreach (var node in nodes)
			{
				AddNode(node);
			}
		}

		public ITweenChain SetOnBegin(System.Action onBegin)
		{
			_onBegin = onBegin;
			return this;
		}
		public ITweenChain SetOnComplete(System.Action onComplete)
		{
			_onComplete = onComplete;
			return this;
		}
		public ITweenChain SetDispose(System.Action onDispose)
		{
			_onDispose = onDispose;
			return this;
		}

		void ITweenNode.OnUpdate(float deltaTime)
		{
			if (Status == ETweenStatus.Idle)
			{
				Status = ETweenStatus.Runing;
				_onBegin?.Invoke();
			}

			if (Status == ETweenStatus.Runing)
			{
				bool isComplete = UpdateChainNodes(deltaTime);
				if (isComplete)
				{
					Status = ETweenStatus.Completed;
					_onComplete?.Invoke();
				}
			}
		}
		void ITweenNode.OnDispose()
		{
			foreach (var node in _nodes)
			{
				node.OnDispose();
			}
			_nodes.Clear();
			_onDispose?.Invoke();
		}
		void ITweenNode.Abort()
		{
			foreach (var node in _nodes)
			{
				node.Abort();
			}
			Status = ETweenStatus.Abort;
		}

		ITweenChain ITweenChain.Append(ITweenNode node)
		{
			AddNode(node);
			return this;
		}

		protected abstract bool UpdateChainNodes(float deltaTime);
	}
}