using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace BehaviorTree
{
	[Export(typeof(BehaviorTreeViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	class BehaviorTreeViewModel
	{
		private ObservableCollection<TreeNodeViewModel> treeNodes = new ObservableCollection<TreeNodeViewModel>();

		public ObservableCollection<TreeNodeViewModel> TreeNodes
		{
			get
			{
				return treeNodes;
			}
			set
			{
				treeNodes = value;
			}
		}

		public TreeNodeViewModel Root
		{
			get
			{
				return treeNodes.Count > 0? treeNodes[0] : null;
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
		}

		public void Remove(TreeNodeViewModel treeNodeViewModel)
		{
			for (int i = treeNodeViewModel.Children.Count - 1; i >= 0; --i)
			{
				Remove(treeNodeViewModel.Children[i]);
			}
			treeNodeViewModel.Parent.Children.Remove(treeNodeViewModel);
			treeNodes.Remove(treeNodeViewModel);
		}
	}
}

