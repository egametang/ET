using System.Collections.Generic;

namespace Tree
{
	public class TreeNode
	{
		private readonly List<int> childIds = new List<int>();

		public double X { get; set; }

		public double Y { get; set; }

		public int Type { get; set; }

		public int Id { get; set; }

		public bool IsFold { get; set; }

		public int ParentId { get; set; }

		public List<int> ChildIds
		{
			get
			{
				return this.childIds;
			}
		}
	}
}