namespace BehaviorTree
{
	public class TreeNode
	{
		public TreeNode(double x, double y)
		{
			X = x;
			Y = y;
		}

		public double X
		{
			get;
			set;
		}

		public double Y
		{
			get;
			set;
		}

		public int Type
		{
			get;
			set;
		}
	}
}