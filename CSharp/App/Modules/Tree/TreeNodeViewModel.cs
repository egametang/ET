using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;

namespace Tree
{
	public class TreeNodeViewModel : BindableBase
	{
		private static int globalNum;
		private static double width = 80;
		private static double height = 50;
		private double x;
		private double y;
		private readonly TreeNodeData treeNodeData;
		private double connectorX2;
		private double connectorY2;
		private double prelim;
		private double modify;
		private double ancestorModify;
		private TreeNodeViewModel parent;

		private ObservableCollection<TreeNodeViewModel> children = new ObservableCollection<TreeNodeViewModel>();

		/// <summary>
		/// 用于初始化根节点
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public TreeNodeViewModel(double x, double y)
		{
			this.x = x;
			this.y = y;
			this.treeNodeData = new TreeNodeData();
			this.treeNodeData.Id = globalNum++;
			this.parent = parent ?? this;
			this.connectorX2 = 0;
			this.connectorY2 = Height / 2;
		}

		public TreeNodeViewModel(TreeNodeViewModel parent)
		{
			this.treeNodeData = new TreeNodeData();
			this.treeNodeData.Id = globalNum++;
			this.parent = parent ?? this;

			this.connectorX2 = Width + this.Parent.X - this.X;
			this.connectorY2 = Height / 2 + this.Parent.Y - this.Y;
		}

		public TreeNodeViewModel(TreeNodeData data, TreeNodeViewModel parent)
		{
			this.treeNodeData = data;
			this.parent = parent ?? this;
			if (this.parent == this)
			{
				this.x = 200;
				this.y = 10;
				this.connectorX2 = 0;
				this.connectorY2 = Height / 2;
			}
			else
			{
				this.connectorX2 = Width + this.Parent.X - this.X;
				this.connectorY2 = Height / 2 + this.Parent.Y - this.Y;
			}
		}

		public TreeNodeData TreeNodeData
		{
			get
			{
				this.treeNodeData.ChildrenId.Clear();
				foreach (TreeNodeViewModel child in children)
				{
					this.treeNodeData.ChildrenId.Add(child.Id);
				}
				this.treeNodeData.ParentId = this.IsRoot? 0 : this.Parent.Id;
				return this.treeNodeData;
			}
		}

		public int Id
		{
			get
			{
				return this.treeNodeData.Id;
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
				return this.x;
			}
			set
			{
				if (Math.Abs(this.x - value) < 0.1)
				{
					return;
				}
				this.x = value;
				this.OnPropertyChanged("X");

				this.ConnectorX2 = Width / 2 + this.Parent.X - this.X;

				foreach (TreeNodeViewModel child in this.Children)
				{
					child.ConnectorX2 = Width / 2 + this.X - child.X;
				}
			}
		}

		public double Y
		{
			get
			{
				return this.y;
			}
			set
			{
				if (Math.Abs(this.Y - value) < 0.1)
				{
					return;
				}

				this.y = value;
				this.OnPropertyChanged("Y");

				this.ConnectorY2 = Height + this.Parent.Y - this.Y;

				foreach (var child in this.Children)
				{
					child.ConnectorY2 = Height + this.Y - child.Y;
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
				return this.treeNodeData.Type;
			}
			set
			{
				if (this.treeNodeData.Type == value)
				{
					return;
				}
				int type = 0;
				this.SetProperty(ref type, value);
				this.treeNodeData.Type = value;
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
				this.OnPropertyChanged("ParentNum");
			}
		}

		/// <summary>
		/// 节点是否折叠
		/// </summary>
		public bool IsFolder
		{
			get
			{
				return this.treeNodeData.IsFold;
			}
			set
			{
				this.treeNodeData.IsFold = value;
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