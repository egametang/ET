using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace BehaviorTree
{
	public class TreeNodeGroup : IEnumerable<ITreeNode>
	{
		Collection<ITreeNode> col = new Collection<ITreeNode>();

		public int Count
		{
			get
			{
				return col.Count;
			}
		}

		public ITreeNode this[int index]
		{
			get { return col[index]; }
		}

		public void Add(ITreeNode tn)
		{
			col.Add(tn);
		}

		internal ITreeNode LeftMost()
		{
			return col.First();
		}

		internal ITreeNode RightMost()
		{
			return col.Last();
		}

		#region IEnumerable<IGraphNode> Members

		public IEnumerator<ITreeNode> GetEnumerator()
		{
			return col.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return col.GetEnumerator();
		}

		#endregion
	}
}
