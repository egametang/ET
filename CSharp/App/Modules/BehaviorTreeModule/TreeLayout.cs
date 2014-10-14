namespace Modules.BehaviorTreeModule
{
    public class TreeLayout
    {
        private readonly TreeViewModel treeViewModel;
        private const double XGap = 20;
        private const double YGap = 10;
        private double rootOrigX;
        private double rootOrigY;
        private double rootOffsetX;
        private double rootOffsetY;

        public TreeLayout(TreeViewModel treeViewModel)
        {
            this.treeViewModel = treeViewModel;
        }

        private TreeNodeViewModel LeftMostOffspring(
                TreeNodeViewModel treeNodeViewModel, int currentLevel, int searchLevel)
        {
            if (currentLevel == searchLevel)
            {
                return treeNodeViewModel;
            }
            for (int i = 0; i < treeNodeViewModel.Children.Count; ++i)
            {
                TreeNodeViewModel child = this.treeViewModel.Get(treeNodeViewModel.Children[i]);
                child.AncestorModify = treeNodeViewModel.Modify + treeNodeViewModel.AncestorModify;
                TreeNodeViewModel offspring = this.LeftMostOffspring(child, currentLevel + 1, searchLevel);
                if (offspring == null)
                {
                    continue;
                }
                return offspring;
            }
            return null;
        }

        private TreeNodeViewModel RightMostOffspring(
                TreeNodeViewModel treeNodeViewModel, int currentLevel, int searchLevel)
        {
            if (currentLevel == searchLevel)
            {
                return treeNodeViewModel;
            }
            for (int i = treeNodeViewModel.Children.Count - 1; i >= 0; --i)
            {
                TreeNodeViewModel child = this.treeViewModel.Get(treeNodeViewModel.Children[i]);
                child.AncestorModify = treeNodeViewModel.Modify + treeNodeViewModel.AncestorModify;
                TreeNodeViewModel offspring = this.RightMostOffspring(child, currentLevel + 1, searchLevel);
                if (offspring == null)
                {
                    continue;
                }
                return offspring;
            }
            return null;
        }

        private void AjustSubTreeGap(TreeNodeViewModel left, TreeNodeViewModel right)
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
                tLeft = this.RightMostOffspring(left, 0, i + 1);
                tRight = this.LeftMostOffspring(right, 0, i + 1);
            }
            right.Modify += offset;
            right.Prelim += offset;
        }

        private void AjustTreeGap(TreeNodeViewModel treeNodeViewModel)
        {
            for (int i = 0; i < treeNodeViewModel.Children.Count - 1; ++i)
            {
                for (int j = i + 1; j < treeNodeViewModel.Children.Count; ++j)
                {
                    TreeNodeViewModel left = this.treeViewModel.Get(treeNodeViewModel.Children[i]);
                    TreeNodeViewModel right = this.treeViewModel.Get(treeNodeViewModel.Children[j]);
                    this.AjustSubTreeGap(left, right);
                }
            }
        }

        private void CalculatePrelimAndModify(TreeNodeViewModel treeNodeViewModel)
        {
            foreach (int childId in treeNodeViewModel.Children)
            {
                TreeNodeViewModel child = this.treeViewModel.Get(childId);
                this.CalculatePrelimAndModify(child);
            }

            double prelim = 0;
            double modify = 0;

            if (treeNodeViewModel.IsLeaf)
            {
                if (treeNodeViewModel.LeftSibling == null)
                {
                    // 如果没有左邻居，不需要设置modify
                    prelim = 0;
                }
                else
                {
                    prelim = treeNodeViewModel.LeftSibling.Prelim + TreeNodeViewModel.Width + XGap;
                }
            }
            else
            {
                // 调整子树间的间距
                this.AjustTreeGap(treeNodeViewModel);
                double childrenCenter = (treeNodeViewModel.FirstChild.Prelim +
                                         treeNodeViewModel.LastChild.Prelim) / 2;
                if (treeNodeViewModel.LeftSibling == null)
                {
                    // 如果没有左邻居，不需要设置modify
                    prelim = childrenCenter;
                }
                else
                {
                    prelim = treeNodeViewModel.LeftSibling.Prelim + TreeNodeViewModel.Width + XGap;
                    modify = prelim - childrenCenter;
                }
            }
            treeNodeViewModel.Prelim = prelim;
            treeNodeViewModel.Modify = modify;

            // Log.Debug("Id: " + treeNodeViewModel.Id + " Prelim: " + treeNodeViewModel.Prelim + " Modify: " +
            // 	treeNodeViewModel.Modify);
        }

        private void CalculateRelativeXAndY(
                TreeNodeViewModel treeNodeViewModel, int level, double totalModify)
        {
            foreach (int childId in treeNodeViewModel.Children)
            {
                TreeNodeViewModel child = this.treeViewModel.Get(childId);
                this.CalculateRelativeXAndY(child, level + 1, treeNodeViewModel.Modify + totalModify);
            }
            if (treeNodeViewModel.IsLeaf)
            {
                treeNodeViewModel.X = treeNodeViewModel.Prelim + totalModify;
            }
            else
            {
                treeNodeViewModel.X = (treeNodeViewModel.FirstChild.X +
                                       treeNodeViewModel.LastChild.X) / 2;
            }
            treeNodeViewModel.Y = level * (TreeNodeViewModel.Height + YGap);
        }

        private void FixXAndY(TreeNodeViewModel treeNode)
        {
            treeNode.X += this.rootOffsetX;
            treeNode.Y += this.rootOffsetY;
            foreach (var childId in treeNode.Children)
            {
                TreeNodeViewModel child = this.treeViewModel.Get(childId);
                this.FixXAndY(child);
            }
        }

        public void ExcuteLayout()
        {
            TreeNodeViewModel root = this.treeViewModel.Root;
            if (root == null)
            {
                return;
            }
            this.rootOrigX = root.X;
            this.rootOrigY = root.Y;
            this.CalculatePrelimAndModify(root);
            this.CalculateRelativeXAndY(root, 0, 0);

            this.rootOffsetX = this.rootOrigX - root.X;
            this.rootOffsetY = this.rootOrigY - root.Y;
            this.FixXAndY(root);
        }
    }
}