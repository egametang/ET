
namespace BehaviorTree
{
	public class TreeNode
	{
		private double x = 0.0;
		private double y = 0.0;
		private int type = 0;

		public TreeNode(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public double X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}

		public double Y
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}

		public int Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}
	}
}
