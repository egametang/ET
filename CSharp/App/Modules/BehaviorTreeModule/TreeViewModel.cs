using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Common.Helper;
using Microsoft.Practices.Prism.Mvvm;
using MongoDB.Bson.Serialization.Attributes;

namespace Modules.BehaviorTreeModule
{
	[BsonDiscriminator("TreeProto", RootClass = true)]
	[Export(typeof (TreeViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
	public class TreeViewModel: BindableBase, ICloneable, ISupportInitialize
	{
		[BsonElement]
		private TreeNodeViewModel root;
		[BsonElement]
		private int treeId;
		[BsonElement]
		private int maxNodeId;
        [BsonIgnore]
		public ObservableCollection<TreeNodeViewModel> allNodes = new ObservableCollection<TreeNodeViewModel>();

		[BsonIgnore]
		public ObservableCollection<TreeNodeViewModel> AllNodes
		{
			get
			{
				return this.allNodes;
			}
		}

		[BsonIgnore]
		public int TreeId
		{
			get
			{
				return this.treeId;
			}
			set
			{
				if (this.treeId == value)
				{
					return;
				}
				this.treeId = value;
				this.OnPropertyChanged("TreeId");
			}
		}

		[BsonIgnore]
		public string Comment
		{
			get
			{
				if (this.root == null)
				{
					return "";
				}
				return this.root.Comment;
			}
			set
			{
				this.OnPropertyChanged("Comment");
			}
		}

		public int MaxNodeId 
		{
			get
			{
				return this.maxNodeId;
			}
			set
			{
				this.maxNodeId = value;
			} 
		}

		[BsonIgnore]
		public TreeNodeViewModel copy;
        
		[BsonIgnore]
		public TreeNodeViewModel Root
		{
			get
			{
				return this.root;
			}
			set
			{
				this.root = value;
			}
		}

		public void Add(TreeNodeViewModel treeNode, TreeNodeViewModel parent)
		{
			// 如果父节点是折叠的,需要先展开父节点
			if (parent != null)
			{
				if (parent.IsFold)
				{
					this.UnFold(parent);
				}
				treeNode.Parent = parent;
				parent.Children.Add(treeNode);
			}
			else
			{
				this.root = treeNode;
			}

			treeNode.TreeViewModel = this;
			allNodes.Add(treeNode);

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		private void GetChildrenIdAndSelf(TreeNodeViewModel treeNodeViewModel, List<TreeNodeViewModel> children)
		{
			children.Add(treeNodeViewModel);
			this.GetAllChildrenId(treeNodeViewModel, children);
		}

		private void GetAllChildrenId(TreeNodeViewModel treeNodeViewModel, List<TreeNodeViewModel> children)
		{
			foreach (TreeNodeViewModel child in treeNodeViewModel.Children)
			{
				children.Add(child);
				this.GetAllChildrenId(child, children);
			}
		}

		public void Remove(TreeNodeViewModel treeNodeViewModel)
		{
			var all = new List<TreeNodeViewModel>();
			this.GetChildrenIdAndSelf(treeNodeViewModel, all);

			foreach (TreeNodeViewModel child in all)
			{
				this.allNodes.Remove(child);
			}

			TreeNodeViewModel parent = treeNodeViewModel.Parent;
			if (parent != null)
			{
				parent.Children.Remove(treeNodeViewModel);
			}

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		private void RecursionMove(TreeNodeViewModel treeNodeViewModel, double offsetX, double offsetY)
		{
			treeNodeViewModel.XX += offsetX;
			treeNodeViewModel.YY += offsetY;
			foreach (TreeNodeViewModel child in treeNodeViewModel.Children)
			{
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
			from.Parent.Children.Remove(from);
			to.Children.Add(from);
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
			var allChild = new List<TreeNodeViewModel>();
			this.GetAllChildrenId(treeNodeViewModel, allChild);

			foreach (TreeNodeViewModel child in allChild)
			{
				this.allNodes.Remove(child);
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

			var allChild = new List<TreeNodeViewModel>();
			this.GetAllChildrenId(treeNodeViewModel, allChild);

			foreach (TreeNodeViewModel child in allChild)
			{
				this.allNodes.Add(child);
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
			int index = parent.Children.IndexOf(treeNodeViewModel);
			if (index == 0)
			{
				return;
			}
			parent.Children.Remove(treeNodeViewModel);
			parent.Children.Insert(index - 1, treeNodeViewModel);

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
			int index = parent.Children.IndexOf(treeNodeViewModel);
			if (index == parent.Children.Count - 1)
			{
				return;
			}
			parent.Children.Remove(treeNodeViewModel);
			parent.Children.Insert(index + 1, treeNodeViewModel);

			TreeLayout treeLayout = new TreeLayout(this);
			treeLayout.ExcuteLayout();
		}

		public void Copy(TreeNodeViewModel copyTreeNodeViewModel)
		{
			this.copy = copyTreeNodeViewModel;
		}

		public void Paste(TreeNodeViewModel pasteTreeNodeViewModel)
		{
			if (this.copy == null)
			{
				return;
			}

			TreeNodeViewModel copyTreeNodeViewModel = this.copy;
			// copy节点不能是paste节点的父级节点
			TreeNodeViewModel tmpNode = pasteTreeNodeViewModel;
			while (tmpNode != null)
			{
				if (tmpNode.IsRoot)
				{
					break;
				}
				if (tmpNode.Id == copyTreeNodeViewModel.Id)
				{
					return;
				}
				tmpNode = tmpNode.Parent;
			}
			this.copy = null;
			this.CopyTree(copyTreeNodeViewModel, pasteTreeNodeViewModel);
		}

		private void CopyTree(TreeNodeViewModel copyTreeNodeViewModel, TreeNodeViewModel parent)
		{
			TreeNodeViewModel newTreeNodeViewModel = (TreeNodeViewModel)copyTreeNodeViewModel.Clone();
			newTreeNodeViewModel.Id = ++this.MaxNodeId;

			this.Add(newTreeNodeViewModel, parent);

			foreach (TreeNodeViewModel child in copyTreeNodeViewModel.Children)
			{
				this.CopyTree(child, newTreeNodeViewModel);
			}
		}

		public object Clone()
		{
			return MongoHelper.FromJson<TreeViewModel>(MongoHelper.ToJson(this));
		}

		private void SetChildParent(TreeNodeViewModel node)
		{
			if (node == null)
			{
				return;
			}
			node.TreeViewModel = this;
            allNodes.Add(node);
			foreach (TreeNodeViewModel child in node.Children)
			{
				child.Parent = node;
				SetChildParent(child);
			}
		}

		public void BeginInit()
		{
		}

		public void EndInit()
		{
			SetChildParent(Root);
			this.Root.XX = 250;
			this.Root.YY = 10;
		}
	}
}