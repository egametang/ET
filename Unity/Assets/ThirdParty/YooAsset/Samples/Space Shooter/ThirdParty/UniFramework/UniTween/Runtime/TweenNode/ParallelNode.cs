
namespace UniFramework.Tween
{
	/// <summary>
	/// 并行执行的复合节点
	/// 说明：节点列表并行执行，所有子节点同时执行，所有节点都结束时复合节点结束。
	/// </summary>
	public sealed class ParallelNode : ChainNode
	{
		protected override bool UpdateChainNodes(float deltaTime)
		{
			bool isComplete = true;
			for (int index = 0; index < _nodes.Count; index++)
			{
				var node = _nodes[index];
				node.OnUpdate(deltaTime);
				if (node.Status == ETweenStatus.Idle || node.Status == ETweenStatus.Runing)
				{
					isComplete = false;
				}
			}
			return isComplete;
		}
	}
}