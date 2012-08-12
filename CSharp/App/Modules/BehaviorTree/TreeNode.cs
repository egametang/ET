namespace Modules.BehaviorTree
{
	public class TreeNode
	{
		public TreeNode(double x, double y)
		{
			this.X = x;
			this.Y = y;
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