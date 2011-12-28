namespace BehaviorTree
{
	public class BehaviorTreeLayout
	{
		private const double XGap = 20;
		private const double YGap = 10;

		private static void CountPrelim(TreeNodeViewModel treeNode)
		{
			if (treeNode.Index == 0)
			{
				treeNode.Prelim = treeNode.Index * (TreeNodeViewModel.Width + XGap);
				return;
			}

			double childrenCenter;
			if (treeNode.Children.Count > 0)
			{
				int maxIndex = treeNode.Children.Count - 1;
				childrenCenter = (treeNode.Children[0].Prelim + treeNode.Children[maxIndex].Prelim) / 2;
			}
			else
			{
				childrenCenter = 0;
			}
			treeNode.Prelim = childrenCenter;
		}

		private static void CountModify(TreeNodeViewModel treeNode, double prelim)
		{
			double childrenCenter = 0;
			if (treeNode.Children.Count > 0)
			{
				int maxIndex = treeNode.Children.Count - 1;
				childrenCenter = (treeNode.Children[0].Prelim + treeNode.Children[maxIndex].Prelim) / 2;
			}
			treeNode.Modify = prelim - childrenCenter;
		}

		private static void CountPrelimAndModify(TreeNodeViewModel treeNode)
		{
			CountPrelim(treeNode);
			CountModify(treeNode, treeNode.Prelim);

			foreach (TreeNodeViewModel node in treeNode.Children)
			{
				CountPrelimAndModify(node);
			}
		}

		private static void AjustTwoSubTreeGap(TreeNodeViewModel left, TreeNodeViewModel right)
		{
			if (left.IsLeaf || right.IsLeaf)
			{
				return;
			}
			double offset = 0;
			TreeNodeViewModel tLeft = left;
			TreeNodeViewModel tRight = right;
			double leftTreeModify = 0;
			double rightTreeModify = 0;
			for (int i = 0;; ++i)
			{
				leftTreeModify += tLeft.Modify;
				rightTreeModify += tRight.Modify;

				tLeft = tLeft.RightMostChild;
				tRight = tRight.LeftMostChild;

				double tGap = (tRight.Prelim + rightTreeModify) - (tLeft.Prelim + leftTreeModify);
				if (tGap - XGap - TreeNodeViewModel.Width > offset)
				{
					offset = tGap - XGap - TreeNodeViewModel.Width;
				}

				if (tLeft.IsLeaf || tRight.IsLeaf)
				{
					break;
				}
			}
			right.Modify += offset;
		}

		private static void AjustTreeGap(TreeNodeViewModel treeNode)
		{
			for (int i = 0; i < treeNode.Children.Count - 1; ++i)
			{
				var left = treeNode.Children[i];
				var right = treeNode.Children[i + 1];
				AjustTwoSubTreeGap(left, right);
			}
		}

		private static void ApplyXY(int level, TreeNodeViewModel treeNode)
		{
			treeNode.X = treeNode.Prelim + treeNode.Modify;
			treeNode.Y = level * (TreeNodeViewModel.Height + YGap);
			++level;
			foreach (TreeNodeViewModel node in treeNode.Children)
			{
				ApplyXY(level, node);
			}
		}

		public static void ExcuteLayout(TreeNodeViewModel root)
		{
			CountPrelimAndModify(root);
			AjustTreeGap(root);
			ApplyXY(0, root);
		}
	}
}