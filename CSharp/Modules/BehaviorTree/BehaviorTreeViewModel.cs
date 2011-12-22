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
			for (int i = 0; i < treeNodeViewModel.Children.Count; ++i)
			{
				Remove(treeNodeViewModel.Children[0]);
			}
			treeNodeViewModel.Parent.Children.Remove(treeNodeViewModel);
			treeNodes.Remove(treeNodeViewModel);
		}
	}
}

