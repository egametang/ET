using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace BehaviorTree
{
	public class TreeNodeViewModel : NotificationObject
	{
		private static int globalNum = 0;
		private readonly int num;
		private const double width = 80;
		private const double height = 50;
		private readonly TreeNode treeNode;
		private ObservableCollection<TreeNodeViewModel> children = new ObservableCollection<TreeNodeViewModel>();
		private double connectorX2;
		private double connectorY2;
		private TreeNodeViewModel parent;

		public TreeNodeViewModel(TreeNode treeNode, TreeNodeViewModel parent)
		{
			num = globalNum++;
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

		public int Num
		{
			get
			{
				return this.num;
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

		public bool IsRoot
		{
			get
			{
				return Parent == this;
			}
		}

		private double prelim;

		public double Prelim
		{
			get
			{
				return this.prelim;
			}
			set
			{
				this.prelim = value;
				RaisePropertyChanged("Prelim");
			}
		}

		private double modify;

		public double Modify
		{
			get
			{
				return this.modify;
			}
			set
			{
				RaisePropertyChanged("Modify");
				this.modify = value;
			}
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

				ConnectorX2 = Width / 2 + Parent.X - X;

				foreach (TreeNodeViewModel child in Children)
				{
					child.ConnectorX2 = Width / 2 + treeNode.X - child.X;
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

				ConnectorY2 = Height + Parent.Y - Y;

				foreach (var child in Children)
				{
					child.ConnectorY2 = Height + treeNode.Y - child.Y;
				}
			}
		}

		public double ConnectorX1
		{
			get
			{
				return Width / 2;
			}
		}

		public double ConnectorY1
		{
			get
			{
				return 0;
			}
		}

		public double ConnectorX2
		{
			get
			{
				return this.IsRoot ? width / 2 : this.connectorX2;
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
				return this.IsRoot ? 0 : this.connectorY2;
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
				if (this.IsRoot)
				{
					return null;
				}

				int index = this.Parent.Children.IndexOf(this);
				return index == 0 ? null : this.Parent.Children[index - 1];
			}
		}

		public int Index
		{
			get
			{
				return this.IsRoot ? 0 : this.Parent.Children.IndexOf(this);
			}
		}

		public TreeNodeViewModel RightMostChild
		{
			get
			{
				if (this.Children.Count == 0)
				{
					return null;
				}

				int maxIndex = this.Children.Count - 1;
				return this.Children[maxIndex];
			}
		}

		public TreeNodeViewModel LeftMostChild
		{
			get
			{
				return this.Children.Count == 0 ? null : this.Children[0];
			}
		}

		public bool IsLeaf
		{
			get
			{
				return this.Children.Count == 0;
			}
		}
	}
}