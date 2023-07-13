
namespace UniFramework.Tween
{
	/// <summary>
	/// 顺序执行的复合节点
	/// 说明：节点列表依次执行，每个子节点结束之后便执行下一个节点，所有节点都结束时复合节点结束。
	/// </summary>
	public sealed class SequenceNode : ChainNode
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
					break;
				}
			}
			return isComplete;
		}
	}
}