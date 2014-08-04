using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;

namespace Tree
{
    public class TreeNodeViewModel: BindableBase
    {
        private static double width = 80;
        private static double height = 50;
        private readonly TreeViewModel treeViewModel;
        private readonly TreeNodeData treeNodeData;
        private double x;
        private double y;
        private double connectorX2;
        private double connectorY2;
        private double prelim;
        private double modify;
        private double ancestorModify;
        private bool isFolder;

        public TreeNodeViewModel(TreeViewModel treeViewModel, double x, double y)
        {
            this.treeViewModel = treeViewModel;
            this.x = x;
            this.y = y;
            this.treeNodeData = new TreeNodeData();
            this.treeNodeData.Id = ++treeViewModel.AllTreeViewModel.MaxNodeId;
            this.treeNodeData.Parent = 0;
            this.connectorX2 = 0;
            this.connectorY2 = Height / 2;
        }

        public TreeNodeViewModel(TreeViewModel treeViewModel, TreeNodeViewModel parent)
        {
            this.treeViewModel = treeViewModel;
            this.treeNodeData = new TreeNodeData();
            this.treeNodeData.Id = ++treeViewModel.AllTreeViewModel.MaxNodeId;
            this.Parent = parent;

            this.connectorX2 = Width + this.Parent.X - this.X;
            this.connectorY2 = Height / 2 + this.Parent.Y - this.Y;
        }

        public TreeNodeViewModel(TreeViewModel treeViewModel, TreeNodeData data)
        {
            this.treeViewModel = treeViewModel;
            this.treeNodeData = data;
            if (this.IsRoot)
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
                this.treeNodeData.Children.Clear();
                foreach (int child in this.Children)
                {
                    this.treeNodeData.Children.Add(child);
                }
                this.treeNodeData.Parent = this.IsRoot? 0 : this.Parent.Id;
                return this.treeNodeData;
            }
        }

        public int Id
        {
            get
            {
                return this.treeNodeData.Id;
            }
            set
            {
                this.treeNodeData.Id = value;
                this.OnPropertyChanged("Id");
            }
        }

        public string Comment
        {
            get
            {
                return this.treeNodeData.Comment;
            }
            set
            {
                this.treeNodeData.Comment = value;
                this.OnPropertyChanged("Comment");
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
                return this.Parent == null;
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

                if (this.Parent != null)
                {
                    this.ConnectorX2 = Width / 2 + this.Parent.X - this.X;
                }

                foreach (var childId in this.Children)
                {
                    TreeNodeViewModel child = this.treeViewModel.Get(childId);
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

                if (this.Parent != null)
                {
                    this.ConnectorY2 = Height + this.Parent.Y - this.Y;
                }

                foreach (var childId in this.Children)
                {
                    TreeNodeViewModel child = this.treeViewModel.Get(childId);
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
                this.treeNodeData.Type = value;
                this.OnPropertyChanged("Type");
            }
        }

        public List<string> Args
        {
            get
            {
                return this.treeNodeData.Args;
            }
            set
            {
                if (this.treeNodeData.Args == value)
                {
                    return;
                }
                this.treeNodeData.Args = value;
                this.OnPropertyChanged("Args");
            }
        }

        public TreeNodeViewModel Parent
        {
            get
            {
                if (this.treeNodeData.Parent == 0)
                {
                    return null;
                }
                TreeNodeViewModel parent = this.treeViewModel.Get(this.treeNodeData.Parent);
                return parent;
            }
            set
            {
                if (value == null)
                {
                    this.treeNodeData.Parent = 0;
                }
                this.treeNodeData.Parent = value.Id;
            }
        }

        /// <summary>
        /// 节点是否折叠
        /// </summary>
        public bool IsFolder
        {
            get
            {
                return this.isFolder;
            }
            set
            {
                if (this.isFolder == value)
                {
                    return;
                }
                this.isFolder = value;
                this.OnPropertyChanged("IsFolder");
            }
        }

        public List<int> Children
        {
            get
            {
                return this.treeNodeData.Children;
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

                int index = this.Parent.Children.IndexOf(this.Id);
                return index == 0? null : this.treeViewModel.Get(this.Parent.Children[index - 1]);
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
                return this.treeViewModel.Get(this.Children[maxIndex]);
            }
        }

        public TreeNodeViewModel FirstChild
        {
            get
            {
                return this.Children.Count == 0? null : this.treeViewModel.Get(this.Children[0]);
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