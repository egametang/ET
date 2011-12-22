using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace BehaviorTree
{
	public class TreeNodeViewModel : NotificationObject
	{
		private TreeNode treeNode;
		private TreeNodeViewModel parent;
		private ObservableCollection<TreeNodeViewModel> children = new ObservableCollection<TreeNodeViewModel>();

		public TreeNodeViewModel(TreeNode treeNode, TreeNodeViewModel parent)
		{
			this.treeNode = treeNode;
			this.parent = parent ?? this;
		}

		public double X
		{
			get
			{
				return treeNode.X;
			}
			set
			{
				if (treeNode.X == value)
				{
					return;
				}
				treeNode.X = value;
				RaisePropertyChanged("X");
			}
		}

		public double Y
		{
			get
			{
				return treeNode.Y;
			}
			set
			{
				if (treeNode.Y == value)
				{
					return;
				}
				treeNode.Y = value;
				RaisePropertyChanged("Y");
			}
		}

		public int Type
		{
			get
			{
				return treeNode.Type;
			}
			set
			{
				if (treeNode.Type == value)
				{
					return;
				}
				treeNode.Type = value;
				RaisePropertyChanged("Type");
			}
		}

		public TreeNodeViewModel Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		public ObservableCollection<TreeNodeViewModel> Children
		{
			get
			{
				return children;
			}
			set
			{
				children = value;
			}
		}
	}
}
