using NLog;

namespace BehaviorTree
{
	public class BehaviorTreeLayout
	{
		private const double XGap = 20;
		private const double YGap = 10;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private static void CountPrelimAndModify(TreeNodeViewModel treeNode)
		{
			foreach (TreeNodeViewModel node in treeNode.Children)
			{
				CountPrelimAndModify(node);
			}

			double prelim = 0;
			double modify = 0;

			double childrenCenter = 0;
			if (!treeNode.IsLeaf)
			{
				childrenCenter = (treeNode.LeftMostChild.Prelim + treeNode.RightMostChild.Prelim) / 2;
			}

			if (treeNode.Index == 0)
			{
				// 如果没有左邻居，不需要设置modify
				prelim = childrenCenter;
			}
			else
			{
				prelim = treeNode.Index * (TreeNodeViewModel.Width + XGap) + treeNode.Parent.LeftMostChild.Prelim;
				modify = prelim - childrenCenter;
			}
			treeNode.Prelim = prelim;
			treeNode.Modify = modify;

			logger.Debug("Num: " + treeNode.Num + " Prelim: " + treeNode.Prelim + " Modify: " + treeNode.Modify);
		}

		private static void AjustTwoSubTreeGap(TreeNodeViewModel left, TreeNodeViewModel right)
		{
			double offset = 0;
			TreeNodeViewModel tLeft = left;
			TreeNodeViewModel tRight = right;
			double leftTreeModify = 0;
			double rightTreeModify = 0;
			for (int i = 0; ; ++i)
			{
				double tGap = (tRight.Prelim + rightTreeModify) - (tLeft.Prelim + leftTreeModify);
				if (XGap + TreeNodeViewModel.Width - tGap > offset)
				{
					offset = XGap + TreeNodeViewModel.Width - tGap;
				}

				if (tLeft.IsLeaf || tRight.IsLeaf)
				{
					break;
				}
				leftTreeModify += tLeft.Modify;
				rightTreeModify += tRight.Modify;
				tLeft = tLeft.RightMostChild;
				tRight = tRight.LeftMostChild;
			}
			right.Modify += offset;
			right.Prelim += offset;
		}

		private static void AjustTreeGap(TreeNodeViewModel treeNode)
		{
			if (treeNode.IsLeaf)
			{
				return;
			}
			foreach (var child in treeNode.Children)
			{
				AjustTreeGap(child);
			}
			for (int i = 0; i < treeNode.Children.Count - 1; ++i)
			{
				var left = treeNode.Children[i];
				var right = treeNode.Children[i + 1];
				AjustTwoSubTreeGap(left, right);
			}
		}

		private static void ApplyXY(TreeNodeViewModel treeNode, int level, double totalModify)
		{
			foreach (TreeNodeViewModel node in treeNode.Children)
			{
				ApplyXY(node, level + 1, treeNode.Modify + totalModify);
			}
			if (treeNode.IsLeaf)
			{
				treeNode.X = treeNode.Prelim + totalModify;
			}
			else
			{
				treeNode.X = (treeNode.LeftMostChild.X + treeNode.RightMostChild.X) / 2;
			}
			treeNode.Y = level * (TreeNodeViewModel.Height + YGap);
		}

		public static void ExcuteLayout(TreeNodeViewModel root)
		{
			if (root == null)
			{
				return;
			}
			CountPrelimAndModify(root);
			AjustTreeGap(root);
			ApplyXY(root, 0, 0);
		}
	}
}