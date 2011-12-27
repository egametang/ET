using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;
using NLog;

namespace BehaviorTree
{
	public class TreeNodeViewModel : NotificationObject
	{
		private double prelim = 0;
		private double modify = 0;
		private static double width = 80;
		private static double height = 50;
		private double connectorX2 = 0;
		private double connectorY2 = 0;
		private TreeNode treeNode;
		private TreeNodeViewModel parent;
		private ObservableCollection<TreeNodeViewModel> children = new ObservableCollection<TreeNodeViewModel>();
		private Logger logger = LogManager.GetCurrentClassLogger();

		public TreeNodeViewModel(TreeNode treeNode, TreeNodeViewModel parent)
		{
			this.treeNode = treeNode;
			this.parent = parent ?? this;
			if (this.parent == this)
			{
				connectorX2 = 0;
				connectorY2 = this.Height / 2;
			}
			else
			{
				connectorX2 = this.Parent.Width + this.Parent.X - this.X;
				connectorY2 = this.Parent.Height / 2 + this.Parent.Y - this.Y;
			}
		}

		public double Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
			}
		}

		public double Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
			}
		}

		public double Prelim
		{
			get
			{
				return prelim;
			}
			set
			{
				prelim = value;
			}
		}

		public double Modify
		{
			get
			{
				return modify;
			}
			set
			{
				modify = value;
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

				this.ConnectorX2 = this.Parent.Width + this.Parent.X - this.X;

				foreach (var child in Children)
				{
					child.ConnectorX2 = this.Width + treeNode.X - child.X;
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

				ConnectorY2 = this.Parent.Height / 2 + this.Parent.Y - this.Y;

				foreach (var child in Children)
				{
					child.ConnectorY2 = this.Height / 2 + treeNode.Y - child.Y;
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
				return this.Height / 2;
			}
		}

		public double ConnectorX2
		{
			get
			{
				if (this.Parent == this)
				{
					return 0;
				}
				return this.connectorX2;
			}
			set
			{
				this.connectorX2 = value;
				RaisePropertyChanged("ConnectorX2");
			}
		}

		public double ConnectorY2
		{
			get
			{
				if (this.Parent == this)
				{
					return this.Height / 2;
				}
				return this.connectorY2;
			}
			set
			{
				this.connectorY2 = value;
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
				if (this.Parent == this)
				{
					return null;
				}

				int index = this.Parent.Children.IndexOf(this);
				if (index == 0)
				{
					return null;
				}
				else
				{
					return this.Parent.Children[index - 1];
				}
			}
		}

		public int Index
		{
			get
			{
				int index = this.Parent.Children.IndexOf(this);
				return index;
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
				return this.Children[Index];
			}
		}

		public TreeNodeViewModel LeftMostChild
		{
			get
			{
				if (this.Children.Count == 0)
				{
					return null;
				}
				return this.Children[0];
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
