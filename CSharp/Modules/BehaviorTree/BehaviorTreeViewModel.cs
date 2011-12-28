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
				return treeNodes;
			}
		}

		private TreeNodeViewModel Root
		{
			get
			{
				return treeNodes.Count == 0 ? null : treeNodes[0];
			}
		}

		public void Add(TreeNode treeNode, TreeNodeViewModel parent)
		{
			var treeNodeViewModel = new TreeNodeViewModel(treeNode, parent);
			treeNodes.Add(treeNodeViewModel);
			if (parent != null)
			{
				parent.Children.Add(treeNodeViewModel);
			}
			BehaviorTreeLayout.ExcuteLayout(Root);
		}

		public void Remove(TreeNodeViewModel treeNodeViewModel)
		{
			for (int i = treeNodeViewModel.Children.Count - 1; i >= 0; --i)
			{
				Remove(treeNodeViewModel.Children[i]);
			}
			treeNodeViewModel.Parent.Children.Remove(treeNodeViewModel);
			treeNodes.Remove(treeNodeViewModel);
			BehaviorTreeLayout.ExcuteLayout(Root);
		}
	}
}