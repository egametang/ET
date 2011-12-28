using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace BehaviorTree
{
	public class TreeNodeViewModel : NotificationObject
	{
		private const double width = 80;
		private const double height = 50;
		private readonly TreeNode treeNode;
		private ObservableCollection<TreeNodeViewModel> children = new ObservableCollection<TreeNodeViewModel>();
		private double connectorX2;
		private double connectorY2;
		private TreeNodeViewModel parent;

		public TreeNodeViewModel(TreeNode treeNode, TreeNodeViewModel parent)
		{
			this.treeNode = treeNode;
			this.parent = parent ?? this;
			if (this.parent == this)
			{
				connectorX2 = 0;
				connectorY2 = Height / 2;
			}
			else
			{
				connectorX2 = Width + Parent.X - X;
				connectorY2 = Height / 2 + Parent.Y - Y;
			}
		}

		public static double Width
		{
			get
			{
				return width;
			}
		}

		public static double Height
		{
			get
			{
				return height;
			}
		}

		public double Prelim
		{
			get;
			set;
		}

		public double Modify
		{
			get;
			set;
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

				ConnectorX2 = Width + Parent.X - X;

				foreach (TreeNodeViewModel child in Children)
				{
					child.ConnectorX2 = Width + treeNode.X - child.X;
				}
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

				ConnectorY2 = Height / 2 + Parent.Y - Y;

				foreach (TreeNodeViewModel child in Children)
				{
					child.ConnectorY2 = Height / 2 + treeNode.Y - child.Y;
				}
			}
		}

		public double ConnectorX1
		{
			get
			{
				return 0;
			}
		}

		public double ConnectorY1
		{
			get
			{
				return Height / 2;
			}
		}

		public double ConnectorX2
		{
			get
			{
				if (Parent == this)
				{
					return 0;
				}
				return connectorX2;
			}
			set
			{
				connectorX2 = value;
				RaisePropertyChanged("ConnectorX2");
			}
		}

		public double ConnectorY2
		{
			get
			{
				if (Parent == this)
				{
					return Height / 2;
				}
				return connectorY2;
			}
			set
			{
				connectorY2 = value;
				RaisePropertyChanged("ConnectorY2");
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

		public TreeNodeViewModel LeftSibling
		{
			get
			{
				if (Parent == this)
				{
					return null;
				}

				int index = Parent.Children.IndexOf(this);
				if (index == 0)
				{
					return null;
				}
				else
				{
					return Parent.Children[index - 1];
				}
			}
		}

		public int Index
		{
			get
			{
				int index = Parent.Children.IndexOf(this);
				return index;
			}
		}

		public TreeNodeViewModel RightMostChild
		{
			get
			{
				if (Children.Count == 0)
				{
					return null;
				}

				int maxIndex = Children.Count - 1;
				return Children[Index];
			}
		}

		public TreeNodeViewModel LeftMostChild
		{
			get
			{
				if (Children.Count == 0)
				{
					return null;
				}
				return Children[0];
			}
		}

		public bool IsLeaf
		{
			get
			{
				return Children.Count == 0;
			}
		}
	}
}