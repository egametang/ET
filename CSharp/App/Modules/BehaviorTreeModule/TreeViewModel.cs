using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Mvvm;

namespace Modules.BehaviorTreeModule
{
	[Export(typeof (TreeViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
	public class TreeViewModel: BindableBase
	{
		private readonly ObservableCollection<TreeNodeViewModel> treeNodes =
				new ObservableCollection<TreeNodeViewModel>();

		private readonly Dictionary<int, TreeNodeViewModel> treeNodeDict =
				new Dictionary<int, TreeNodeViewModel>();

		public ObservableCollection<TreeNodeViewModel> TreeNodes
		{
			get
			{
				return this.treeNodes;
			}
		}

		public int TreeId { get; private set; }

		public TreeViewModel(AllTreeViewModel allTreeViewModel)
		{
			this.AllTreeViewModel = allTreeViewModel;
			this.TreeId = ++allTreeViewModel.MaxTreeId;
			TreeNodeViewModel treeNodeViewModel = new TreeNodeViewModel(this, 300, 100);
			this.treeNodes.Add(treeNodeViewModel);
			this.treeNodeDict[treeNodeViewModel.Id] = treeNodeViewModel;

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		public TreeViewModel(AllTreeViewModel allTreeViewModel, List<TreeNodeData> treeNodeDatas)
		{
			this.AllTreeViewModel = allTreeViewModel;
			this.TreeId = treeNodeDatas[0].TreeId;
			foreach (TreeNodeData treeNodeData in treeNodeDatas)
			{
				TreeNodeViewModel treeNodeViewModel = new TreeNodeViewModel(this, treeNodeData);
				this.treeNodes.Add(treeNodeViewModel);
				this.treeNodeDict[treeNodeViewModel.Id] = treeNodeViewModel;
			}
			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		public TreeViewModel(AllTreeViewModel allTreeViewModel, TreeViewModel copyTree)
		{
			this.AllTreeViewModel = allTreeViewModel;
			this.TreeId = ++this.AllTreeViewModel.MaxTreeId;
			// 旧id和新id的映射关系
			var idMapping = new Dictionary<int, int>();
			idMapping[0] = 0;
			List<TreeNodeData> treeNodeDatas = copyTree.GetDatas();
			foreach (TreeNodeData treeNodeData in treeNodeDatas)
			{
				int newId = ++this.AllTreeViewModel.MaxNodeId;
				idMapping[treeNodeData.Id] = newId;
				treeNodeData.Id = newId;
				treeNodeData.TreeId = this.TreeId;
			}

			foreach (TreeNodeData treeNodeData in treeNodeDatas)
			{
				treeNodeData.Parent = idMapping[treeNodeData.Parent];
				for (int i = 0; i < treeNodeData.Children.Count; ++i)
				{
					treeNodeData.Children[i] = idMapping[treeNodeData.Children[i]];
				}
			}

			foreach (TreeNodeData treeNodeData in treeNodeDatas)
			{
				TreeNodeViewModel treeNodeViewModel = new TreeNodeViewModel(this, treeNodeData);
				this.treeNodes.Add(treeNodeViewModel);
				this.treeNodeDict[treeNodeViewModel.Id] = treeNodeViewModel;
			}
			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		public List<TreeNodeData> GetDatas()
		{
			var treeNodeDatas = new List<TreeNodeData>();
			foreach (TreeNodeViewModel treeNodeViewModel in this.treeNodes)
			{
				TreeNodeData treeNodeData = (TreeNodeData)treeNodeViewModel.Data.Clone();
				treeNodeDatas.Add(treeNodeData);
			}
			return treeNodeDatas;
		}

		public AllTreeViewModel AllTreeViewModel { get; set; }

		public TreeNodeViewModel Root
		{
			get
			{
				return this.treeNodes.Count == 0? null : this.treeNodes[0];
			}
		}

		public TreeNodeViewModel Get(int id)
		{
			TreeNodeViewModel node;
			this.treeNodeDict.TryGetValue(id, out node);
			return node;
		}

		public void Add(TreeNodeViewModel treeNode, TreeNodeViewModel parent)
		{
			// 如果父节点是折叠的,需要先展开父节点
			if (parent != null && parent.IsFold)
			{
				this.UnFold(parent);
			}

			this.treeNodes.Add(treeNode);
			this.treeNodeDict[treeNode.Id] = treeNode;

			if (parent != null)
			{
				parent.Children.Add(treeNode.Id);
			}

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		private void GetChildrenIdAndSelf(TreeNodeViewModel treeNodeViewModel, List<int> children)
		{
			children.Add(treeNodeViewModel.Id);
			this.GetAllChildrenId(treeNodeViewModel, children);
		}

		private void GetAllChildrenId(TreeNodeViewModel treeNodeViewModel, List<int> children)
		{
			foreach (int childId in treeNodeViewModel.Children)
			{
				TreeNodeViewModel child = this.Get(childId);
				children.Add(child.Id);
				this.GetAllChildrenId(child, children);
			}
		}

		public void Remove(TreeNodeViewModel treeNodeViewModel)
		{
			var allId = new List<int>();
			this.GetChildrenIdAndSelf(treeNodeViewModel, allId);

			foreach (int childId in allId)
			{
				TreeNodeViewModel child = this.Get(childId);
				this.treeNodes.Remove(child);
				this.treeNodes.Remove(treeNodeViewModel);
			}

			TreeNodeViewModel parent = treeNodeViewModel.Parent;
			if (parent != null)
			{
				parent.Children.Remove(treeNodeViewModel.Id);
			}

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		private void RecursionMove(TreeNodeViewModel treeNodeViewModel, double offsetX, double offsetY)
		{
			treeNodeViewModel.X += offsetX;
			treeNodeViewModel.Y += offsetY;
			foreach (int childId in treeNodeViewModel.Children)
			{
				TreeNodeViewModel child = this.Get(childId);
				this.RecursionMove(child, offsetX, offsetY);
			}
		}

		public void MoveToPosition(double offsetX, double offsetY)
		{
			this.RecursionMove(this.Root, offsetX, offsetY);
		}

		public void MoveToNode(TreeNodeViewModel from, TreeNodeViewModel to)
		{
			// from节点不能是to节点的父级节点
			TreeNodeViewModel tmpNode = to;
			while (tmpNode != null)
			{
				if (tmpNode.IsRoot)
				{
					break;
				}
				if (tmpNode.Id == from.Id)
				{
					return;
				}
				tmpNode = tmpNode.Parent;
			}

			if (from.IsFold)
			{
				this.UnFold(from);
			}

			if (to.IsFold)
			{
				this.UnFold(to);
			}
			from.Parent.Children.Remove(from.Id);
			to.Children.Add(from.Id);
			from.Parent = to;
			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		/// <summary>
		/// 折叠节点
		/// </summary>
		/// <param name="treeNodeViewModel"></param>
		public void Fold(TreeNodeViewModel treeNodeViewModel)
		{
			var allChildId = new List<int>();
			this.GetAllChildrenId(treeNodeViewModel, allChildId);

			foreach (int childId in allChildId)
			{
				TreeNodeViewModel child = this.Get(childId);
				this.treeNodes.Remove(child);
			}

			treeNodeViewModel.IsFold = true;

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		/// <summary>
		/// 展开节点,一级一级展开,一次只展开下层子节点,比如下层节点是折叠的,那下下层节点不展开
		/// </summary>
		/// <param name="treeNodeViewModel"></param>
		public void UnFold(TreeNodeViewModel treeNodeViewModel)
		{
			treeNodeViewModel.IsFold = false;

			var allChildId = new List<int>();
			this.GetAllChildrenId(treeNodeViewModel, allChildId);

			foreach (int childId in allChildId)
			{
				TreeNodeViewModel child = this.Get(childId);
				this.treeNodes.Add(child);
			}

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		public void MoveLeft(TreeNodeViewModel treeNodeViewModel)
		{
			if (treeNodeViewModel.IsRoot)
			{
				return;
			}
			TreeNodeViewModel parent = treeNodeViewModel.Parent;
			int index = parent.Children.IndexOf(treeNodeViewModel.Id);
			if (index == 0)
			{
				return;
			}
			parent.Children.Remove(treeNodeViewModel.Id);
			parent.Children.Insert(index - 1, treeNodeViewModel.Id);

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		public void MoveRight(TreeNodeViewModel treeNodeViewModel)
		{
			if (treeNodeViewModel.IsRoot)
			{
				return;
			}
			TreeNodeViewModel parent = treeNodeViewModel.Parent;
			int index = parent.Children.IndexOf(treeNodeViewModel.Id);
			if (index == parent.Children.Count - 1)
			{
				return;
			}
			parent.Children.Remove(treeNodeViewModel.Id);
			parent.Children.Insert(index + 1, treeNodeViewModel.Id);

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}
	}
}