using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.ViewModel;

namespace Tree
{
	public class TreeNodeViewModel : BindableBase
	{
		private static int globalNum;
		private readonly int num;
		private static double width = 80;
		private static double height = 50;
		private readonly TreeNode treeNode;
		private double connectorX2;
		private double connectorY2;
		private double prelim;
		private double modify;
		private double ancestorModify;
		private TreeNodeViewModel parent;

		private ObservableCollection<TreeNodeViewModel> children =
			new ObservableCollection<TreeNodeViewModel>();

		public TreeNodeViewModel(TreeNode treeNode, TreeNodeViewModel parent)
		{
			this.num = globalNum++;
			this.treeNode = treeNode;
			this.parent = parent ?? this;
			if (this.parent == this)
			{
				this.connectorX2 = 0;
				this.connectorY2 = Height / 2;
			}
			else
			{
				this.connectorX2 = Width + this.Parent.X - this.X;
				this.connectorY2 = Height / 2 + this.Parent.Y - this.Y;
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
			set
			{
				width = value;
			}
		}

		public static double Height
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

		public bool IsRoot
		{
			get
			{
				return this.Parent == this;
			}
		}

		public double Prelim
		{
			get
			{
				return this.prelim;
			}
			set
			{
				this.prelim = value;
			}
		}

		public double Modify
		{
			get
			{
				return this.modify;
			}
			set
			{
				this.modify = value;
			}
		}

		public double X
		{
			get
			{
				return this.treeNode.X;
			}
			set
			{
				if (Math.Abs(this.treeNode.X - value) < 0.1)
				{
					return;
				}
				double x = 0;
				this.SetProperty(ref x, value);
				this.treeNode.X = value;

				this.ConnectorX2 = Width / 2 + this.Parent.X - this.X;

				foreach (TreeNodeViewModel child in this.Children)
				{
					child.ConnectorX2 = Width / 2 + this.treeNode.X - child.X;
				}
			}
		}

		public double Y
		{
			get
			{
				return this.treeNode.Y;
			}
			set
			{
				if (Math.Abs(this.treeNode.Y - value) < 0.1)
				{
					return;
				}
				double y = 0;
				this.SetProperty(ref y, value);
				this.treeNode.Y = value;

				this.ConnectorY2 = Height + this.Parent.Y - this.Y;

				foreach (var child in this.Children)
				{
					child.ConnectorY2 = Height + this.treeNode.Y - child.Y;
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
				return this.IsRoot? Width / 2 : this.connectorX2;
			}
			set
			{
				this.SetProperty(ref this.connectorX2, value);
			}
		}

		public double ConnectorY2
		{
			get
			{
				return this.IsRoot? 0 : this.connectorY2;
			}
			set
			{
				this.SetProperty(ref this.connectorY2, value);
			}
		}

		public int Type
		{
			get
			{
				return this.treeNode.Type;
			}
			set
			{
				if (this.treeNode.Type == value)
				{
					return;
				}
				int type = 0;
				this.SetProperty(ref type, value);
				this.treeNode.Type = value;
			}
		}

		public TreeNodeViewModel Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;
			}
		}

		public ObservableCollection<TreeNodeViewModel> Children
		{
			get
			{
				return this.children;
			}
			set
			{
				this.children = value;
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
				return index == 0? null : this.Parent.Children[index - 1];
			}
		}

		public TreeNodeViewModel LastChild
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

		public TreeNodeViewModel FirstChild
		{
			get
			{
				return this.Children.Count == 0? null : this.Children[0];
			}
		}

		public bool IsLeaf
		{
			get
			{
				return this.Children.Count == 0;
			}
		}

		public double AncestorModify
		{
			get
			{
				return this.ancestorModify;
			}
			set
			{
				this.ancestorModify = value;
			}
		}
	}
}