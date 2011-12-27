using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
	class BehaviorTreeLayout
	{
		private static double xGap = 20;
		private static double yGap = 10;
		private TreeNodeViewModel root = null;

		public BehaviorTreeLayout(TreeNodeViewModel root)
		{
			this.root = root;
		}

		public static double XGap
		{
			get
			{
				return BehaviorTreeLayout.xGap;
			}
			set
			{
				BehaviorTreeLayout.xGap = value;
			}
		}

		public static double YGap
		{
			get
			{
				return BehaviorTreeLayout.yGap;
			}
			set
			{
				BehaviorTreeLayout.yGap = value;
			}
		}

		public void CountPrelim(TreeNodeViewModel treeNode)
		{
			
			if (treeNode.Index == 0)
			{
				treeNode.Prelim = treeNode.Index * (treeNode.Width + BehaviorTreeLayout.XGap);
				return;
			}

			double childrenCenter = 0;
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

		public void CountModify(TreeNodeViewModel treeNode, double prelim)
		{
			double childrenCenter = 0;
			if (treeNode.Children.Count > 0)
			{
				int maxIndex = treeNode.Children.Count - 1;
				childrenCenter = (treeNode.Children[0].Prelim + treeNode.Children[maxIndex].Prelim) / 2;
			}
			treeNode.Modify = prelim - childrenCenter;
		}

		public void CountPrelimAndModify(TreeNodeViewModel treeNode)
		{
			CountPrelim(treeNode);
			CountModify(treeNode, treeNode.Prelim);

			foreach (var node in treeNode.Children)
			{
				CountPrelimAndModify(node);
			}
		}

		public void AjustTwoSubTreeGap(TreeNodeViewModel left, TreeNodeViewModel right)
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
			for (int i = 0; ; ++i)
			{
				if (tLeft.IsLeaf || tRight.IsLeaf)
				{
					right.Modify += offset;
					return;
				}

				leftTreeModify += tLeft.Modify;
				rightTreeModify += tRight.Modify;

				tLeft = tLeft.RightMostChild;
				tRight = tRight.LeftMostChild;

				double tGap = (tRight.Prelim + rightTreeModify) - (tLeft.Prelim + leftTreeModify);
				if (tGap - BehaviorTreeLayout.XGap > offset)
				{
					offset = tGap - BehaviorTreeLayout.XGap;
				}
			}
		}

		public void AjustTreeGap(TreeNodeViewModel treeNode)
		{
			for (int i = 0; i < treeNode.Children.Count - 1; ++i)
			{
				TreeNodeViewModel left = treeNode.Children[i];
				TreeNodeViewModel right = treeNode.Children[i + 1];
				AjustTwoSubTreeGap(left, right);
			}
		}

		public void ApplyXY(TreeNodeViewModel treeNode)
		{
			treeNode.X = treeNode.Prelim;
			double realModify = treeNode.Prelim - treeNode.Modify;
			foreach (var node in treeNode.Children)
			{

			}
		}

		public void ExcuteLayout()
		{
			CountPrelimAndModify(root);
			AjustTreeGap(root);
			ApplyXY(root);
		}
	}
}
