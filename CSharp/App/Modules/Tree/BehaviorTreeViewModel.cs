using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace Tree
{
	[Export(contractType: typeof (BehaviorTreeViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class BehaviorTreeViewModel
	{
		private readonly ObservableCollection<TreeNodeViewModel> treeNodes =
			new ObservableCollection<TreeNodeViewModel>();

		// 保存折叠节点的孩子节点
		private readonly Dictionary<TreeNodeViewModel, ObservableCollection<TreeNodeViewModel>>
			folderNodeChildren = new Dictionary<TreeNodeViewModel, ObservableCollection<TreeNodeViewModel>>();

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
				return this.treeNodes.Count == 0? null : this.treeNodes[0];
			}
		}

		public void Add(TreeNode treeNode, TreeNodeViewModel parent)
		{
			// 如果父节点是折叠的,需要先展开父节点
			if (parent != null && parent.IsFolder)
			{
				UnFold(parent);
			}
			var treeNodeViewModel = new TreeNodeViewModel(treeNode, parent);
			this.treeNodes.Add(treeNodeViewModel);
			if (parent != null)
			{
				parent.Children.Add(treeNodeViewModel);
			}
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}

		private void RecursionRemove(TreeNodeViewModel treeNodeViewModel)
		{
			for (int i = treeNodeViewModel.Children.Count - 1; i >= 0; --i)
			{
				this.RecursionRemove(treeNodeViewModel.Children[i]);
			}
			this.treeNodes.Remove(treeNodeViewModel);
		}

		public void Remove(TreeNodeViewModel treeNodeViewModel)
		{
			this.RecursionRemove(treeNodeViewModel);
			treeNodeViewModel.Parent.Children.Remove(treeNodeViewModel);
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}

		private void RecursionMove(TreeNodeViewModel treeNodeViewModel, double offsetX, double offsetY)
		{
			treeNodeViewModel.X += offsetX;
			treeNodeViewModel.Y += offsetY;
			foreach (var node in treeNodeViewModel.Children)
			{
				this.RecursionMove(node, offsetX, offsetY);
			}
		}

		public void MoveToPosition(double offsetX, double offsetY)
		{
			this.RecursionMove(this.Root, offsetX, offsetY);
		}

		public void MoveToNode(TreeNodeViewModel from, TreeNodeViewModel to)
		{
			from.Parent.Children.Remove(from);
			to.Children.Add(from);
			from.Parent = to;
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}

		/// <summary>
		/// 折叠节点
		/// </summary>
		/// <param name="treeNodeViewModel"></param>
		public void Fold(TreeNodeViewModel treeNodeViewModel)
		{
			this.folderNodeChildren[treeNodeViewModel] = treeNodeViewModel.Children;
			foreach (var node in treeNodeViewModel.Children)
			{
				this.RecursionRemove(node);
			}
			treeNodeViewModel.IsFolder = true;
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}

		/// <summary>
		/// 展开节点
		/// </summary>
		/// <param name="treeNodeViewModel"></param>
		public void UnFold(TreeNodeViewModel treeNodeViewModel)
		{
			ObservableCollection<TreeNodeViewModel> children = this.folderNodeChildren[treeNodeViewModel];
			foreach (var tn in children)
			{
				this.RecursionAdd(tn);
			}
			treeNodeViewModel.IsFolder = false;
			BehaviorTreeLayout.ExcuteLayout(this.Root);
		}

		private void RecursionAdd(TreeNodeViewModel treeNodeViewModel)
		{
			this.treeNodes.Add(treeNodeViewModel);
			ObservableCollection<TreeNodeViewModel> children = treeNodeViewModel.Children;
			foreach (var tn in children)
			{
				this.RecursionAdd(tn);
			}
		}
	}
}