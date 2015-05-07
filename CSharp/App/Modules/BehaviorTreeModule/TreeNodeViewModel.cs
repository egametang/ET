using System;
using System.Collections.Generic;
using Common.Helper;
using Microsoft.Practices.Prism.Mvvm;
using MongoDB.Bson.Serialization.Attributes;

namespace Modules.BehaviorTreeModule
{
	[BsonDiscriminator("NodeProto", RootClass = true)]
	public class TreeNodeViewModel: BindableBase, ICloneable
	{
		[BsonElement]
		private int id;
		[BsonElement]
		private int type;
		[BsonElement, BsonIgnoreIfNull]
		private List<string> args;
		[BsonElement, BsonIgnoreIfNull]
		private string comment;
		[BsonElement, BsonIgnoreIfNull]
		private List<TreeNodeViewModel> children = new List<TreeNodeViewModel>();

		private TreeNodeViewModel parent;

		private static double width = 80;
		private static double height = 50;

		private double x;
		private double y;
		private double connectorX2;
		private double connectorY2;
		private double prelim;
		private double modify;
		private double ancestorModify;
		private bool isFold;

		[BsonIgnore]
		public TreeViewModel TreeViewModel { get; set; }

		public TreeNodeViewModel(TreeViewModel treeViewModel, double x, double y)
		{
			this.TreeViewModel = treeViewModel;
			this.x = x;
			this.y = y;
			this.id = ++treeViewModel.MaxNodeId;
			this.connectorX2 = 0;
			this.connectorY2 = Height / 2;
		}

		public TreeNodeViewModel(TreeViewModel treeViewModel, TreeNodeViewModel parent)
		{
			this.TreeViewModel = treeViewModel;
			this.Id = ++treeViewModel.MaxNodeId;
			this.Parent = parent;

			this.connectorX2 = Width + this.Parent.XX - this.XX;
			this.connectorY2 = Height / 2 + this.Parent.YY - this.YY;
		}

		[BsonIgnore]
		public int Id
		{
			get
			{
				return this.id;
			}
			set
			{
				if (this.id == value)
				{
					return;
				}
				this.id = value;
				this.OnPropertyChanged("Id");
			}
		}

		[BsonIgnore]
		public string Comment
		{
			get
			{
				return this.comment;
			}
			set
			{
				if (this.comment == value)
				{
					return;
				}
				this.comment = value;
				if (this.IsRoot)
				{
					this.TreeViewModel.Comment = this.comment;
				}
				this.OnPropertyChanged("Comment");
			}
		}

		[BsonIgnore]
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

		[BsonIgnore]
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

		[BsonIgnore]
		public bool IsRoot
		{
			get
			{
				return this.Parent == null;
			}
		}

		[BsonIgnore]
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

		[BsonIgnore]
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

		[BsonIgnore]
		public double XX
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
				this.OnPropertyChanged("XX");

				if (this.Parent != null)
				{
					this.ConnectorX2 = Width / 2 + this.Parent.XX - this.XX;
				}

				foreach (TreeNodeViewModel child in this.Children)
				{
					child.ConnectorX2 = Width / 2 + this.XX - child.XX;
				}
			}
		}

		[BsonIgnore]
		public double YY
		{
			get
			{
				return this.y;
			}
			set
			{
				if (Math.Abs(this.YY - value) < 0.1)
				{
					return;
				}

				this.y = value;
				this.OnPropertyChanged("YY");

				if (this.Parent != null)
				{
					this.ConnectorY2 = Height + this.Parent.YY - this.YY;
				}

				foreach (TreeNodeViewModel child in this.Children)
				{
					child.ConnectorY2 = Height + this.YY - child.YY;
				}
			}
		}

		[BsonIgnore]
		public double ConnectorX1
		{
			get
			{
				return Width / 2;
			}
		}

		[BsonIgnore]
		public double ConnectorY1
		{
			get
			{
				return 0;
			}
		}

		[BsonIgnore]
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

		[BsonIgnore]
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

		[BsonIgnore]
		public int Type
		{
			get
			{
				return this.type;
			}
			set
			{
				if (this.type == value)
				{
					return;
				}
				this.type = value;
				this.OnPropertyChanged("Type");
			}
		}

		[BsonIgnore]
		public List<string> Args
		{
			get
			{
				return this.args;
			}
			set
			{
				if (this.args == value)
				{
					return;
				}
				this.args = value;
				this.OnPropertyChanged("Args");
			}
		}

		[BsonIgnore]
		public TreeNodeViewModel Parent
		{
			get
			{
				return parent;
			}
			set
			{
				this.parent = value;
			}
		}

		/// <summary>
		/// 节点是否折叠
		/// </summary>
		[BsonIgnore]
		public bool IsFold
		{
			get
			{
				return this.isFold;
			}
			set
			{
				if (this.isFold == value)
				{
					return;
				}
				this.isFold = value;
				this.OnPropertyChanged("IsFold");
			}
		}

		[BsonIgnore]
		public List<TreeNodeViewModel> Children
		{
			get
			{
				if (this.isFold)
				{
					return new List<TreeNodeViewModel>();
				}
				return this.children;
			}
			set
			{
				this.children = value;
			}
		}

		[BsonIgnore]
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

		[BsonIgnore]
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

		[BsonIgnore]
		public TreeNodeViewModel FirstChild
		{
			get
			{
				return this.Children.Count == 0? null : this.Children[0];
			}
		}

		[BsonIgnore]
		public bool IsLeaf
		{
			get
			{
				return this.Children.Count == 0;
			}
		}

		[BsonIgnore]
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

		public object Clone()
		{
			return MongoHelper.FromJson<TreeNodeViewModel>(MongoHelper.ToJson(this));
		}
	}
}