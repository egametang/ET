
namespace UniFramework.Tween
{
	/// <summary>
	/// 随机执行的复合节点
	/// 说明：节点列表随机执行，在随机节点结束后复合节点结束。
	/// </summary>
	public sealed class SelectorNode : ChainNode
	{
		private ITweenNode _selectNode = null;

		protected override bool UpdateChainNodes(float deltaTime)
		{
			if (_selectNode == null)
			{
				if (_nodes.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, _nodes.Count);
					_selectNode = _nodes[index];
				}
			}

			if (_selectNode != null)
			{
				_selectNode.OnUpdate(deltaTime);
				if (_selectNode.Status == ETweenStatus.Idle || _selectNode.Status == ETweenStatus.Runing)
					return false;
				else
					return true;
			}

			// 如果没有执行节点直接返回完成
			return true;
		}
	}
}