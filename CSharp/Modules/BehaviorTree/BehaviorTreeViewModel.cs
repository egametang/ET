using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace BehaviorTree
{
	[Export(typeof (BehaviorTreeViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
	internal class BehaviorTreeViewModel
	{
		private readonly ObservableCollection<TreeNodeViewModel> treeNodes = new ObservableCollection<TreeNodeViewModel>();

		public ObservableCollection<TreeNodeViewModel> TreeNodes
		{
			get
			{
				return this.treeNodes;
			}
		}

		private TreeNodeViewModel Root
		{
			get
			{
				return this.treeNodes.Count == 0 ? null : this.treeNodes[0];
			}
		}

		public void Add(TreeNode treeNode, TreeNodeViewModel parent)
		{
			var treeNodeViewModel = new TreeNodeViewModel(treeNode, parent);
			this.treeNodes.Add(treeNodeViewModel);
			if (parent != null)
			{
				parent.Children.Add(treeNodeViewModel);
			}
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}

		public void Remove(TreeNodeViewModel treeNodeViewModel)
		{
			for (int i = treeNodeViewModel.Children.Count - 1; i >= 0; --i)
			{
				this.Remove(treeNodeViewModel.Children[i]);
			}
			treeNodeViewModel.Parent.Children.Remove(treeNodeViewModel);
			this.treeNodes.Remove(treeNodeViewModel);
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}
	}
}