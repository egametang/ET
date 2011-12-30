using NLog;

namespace BehaviorTree
{
	public static class BehaviorTreeLayout
	{
		private const double XGap = 20;
		private const double YGap = 10;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static TreeNodeViewModel LeftMostOffspring(TreeNodeViewModel treeNode, int currentLevel, int searchLevel)
		{
			if (currentLevel == searchLevel)
			{
				return treeNode;
			}
			for (int i = 0; i < treeNode.Children.Count; ++i)
			{
				var child = treeNode.Children[i];
				child.AncestorModify = treeNode.Modify + treeNode.AncestorModify;
				var offspring = LeftMostOffspring(child, currentLevel + 1, searchLevel);
				if (offspring == null)
				{
					continue;
				}
				return offspring;
			}
			return null;
		}

		public static TreeNodeViewModel RightMostOffspring(TreeNodeViewModel treeNode, int currentLevel, int searchLevel)
		{
			if (currentLevel == searchLevel)
			{
				return treeNode;
			}
			for (int i = treeNode.Children.Count - 1; i >= 0; --i)
			{
				var child = treeNode.Children[i];
				child.AncestorModify = treeNode.Modify + treeNode.AncestorModify;
				var offspring = RightMostOffspring(child, currentLevel + 1, searchLevel);
				if (offspring == null)
				{
					continue;
				}
				return offspring;
			}
			return null;
		}

		private static void AjustSubTreeGap(TreeNodeViewModel left, TreeNodeViewModel right)
		{
			double offset = 0;
			TreeNodeViewModel tLeft = left;
			TreeNodeViewModel tRight = right;
			left.AncestorModify = 0;
			right.AncestorModify = 0;
			for (int i = 0; tLeft != null && tRight != null; ++i)
			{
				double tGap = (tRight.Prelim + tRight.AncestorModify) - (tLeft.Prelim + tLeft.AncestorModify);
				if (XGap + TreeNodeViewModel.Width - tGap > offset)
				{
					offset = XGap + TreeNodeViewModel.Width - tGap;
				}
				tLeft = RightMostOffspring(left, 0, i + 1);
				tRight = LeftMostOffspring(right, 0, i + 1);
			}
			right.Modify += offset;
			right.Prelim += offset;
		}

		private static void AjustTreeGap(TreeNodeViewModel treeNode)
		{
			for (int i = 0; i < treeNode.Children.Count - 1; ++i)
			{
				for (int j = i + 1; j < treeNode.Children.Count; ++j)
				{
					var left = treeNode.Children[i];
					var right = treeNode.Children[j];
					AjustSubTreeGap(left, right);
				}
			}
		}

		private static void CalculatePrelimAndModify(TreeNodeViewModel treeNode)
		{
			foreach (TreeNodeViewModel node in treeNode.Children)
			{
				CalculatePrelimAndModify(node);
			}

			double prelim = 0;
			double modify = 0;

			if (treeNode.IsLeaf)
			{
				if (treeNode.LeftSibling == null)
				{
					// 如果没有左邻居，不需要设置modify
					prelim = 0;
				}
				else
				{
					prelim = treeNode.LeftSibling.Prelim + TreeNodeViewModel.Width + XGap;
				}
			}
			else
			{
				// 调整子树间的间距
				AjustTreeGap(treeNode);
				double childrenCenter = (treeNode.FirstChild.Prelim + treeNode.LastChild.Prelim) / 2;
				if (treeNode.LeftSibling == null)
				{
					// 如果没有左邻居，不需要设置modify
					prelim = childrenCenter;
				}
				else
				{
					prelim = treeNode.LeftSibling.Prelim + TreeNodeViewModel.Width + XGap;
					modify = prelim - childrenCenter;
				}
			}
			treeNode.Prelim = prelim;
			treeNode.Modify = modify;

			logger.Debug("Num: " + treeNode.Num + " Prelim: " + treeNode.Prelim + " Modify: " + treeNode.Modify);
		}

		private static void CalculateXAndY(TreeNodeViewModel treeNode, int level, double totalModify)
		{
			foreach (TreeNodeViewModel node in treeNode.Children)
			{
				CalculateXAndY(node, level + 1, treeNode.Modify + totalModify);
			}
			if (treeNode.IsLeaf)
			{
				treeNode.X = treeNode.Prelim + totalModify;
			}
			else
			{
				treeNode.X = (treeNode.FirstChild.X + treeNode.LastChild.X) / 2;
			}
			treeNode.Y = level * (TreeNodeViewModel.Height + YGap);
		}

		public static void ExcuteLayout(TreeNodeViewModel root)
		{
			if (root == null)
			{
				return;
			}
			CalculatePrelimAndModify(root);
			CalculateXAndY(root, 0, 0);
		}
	}
}