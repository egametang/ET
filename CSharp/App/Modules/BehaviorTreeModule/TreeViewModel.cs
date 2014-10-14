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

        public int TreeId { get; set; }

        public TreeViewModel(AllTreeViewModel allTreeViewModel)
        {
            this.AllTreeViewModel = allTreeViewModel;
            this.TreeId = ++allTreeViewModel.MaxTreeId;
            TreeNodeViewModel treeNodeViewModel = new TreeNodeViewModel(this, 300, 100);
            this.treeNodes.Add(treeNodeViewModel);
            this.treeNodeDict[treeNodeViewModel.Id] = treeNodeViewModel;

            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        public TreeViewModel(List<TreeNodeData> treeNodeDatas)
        {
            foreach (TreeNodeData treeNodeData in treeNodeDatas)
            {
                TreeNodeViewModel treeNodeViewModel = new TreeNodeViewModel(this, treeNodeData);
                this.treeNodes.Add(treeNodeViewModel);
                this.treeNodeDict[treeNodeViewModel.Id] = treeNodeViewModel;
            }
            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        public List<TreeNodeData> GetDatas()
        {
            List<TreeNodeData> treeNodeDatas = new List<TreeNodeData>();
            foreach (TreeNodeViewModel treeNodeViewModel in this.treeNodes)
            {
                treeNodeDatas.Add(treeNodeViewModel.Data);
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

            var treeLayout = new TreeLayout(this);
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
            List<int> allId = new List<int>();
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

            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        private void RecursionMove(
                TreeNodeViewModel treeNodeViewModel, double offsetX, double offsetY)
        {
            treeNodeViewModel.X += offsetX;
            treeNodeViewModel.Y += offsetY;
            foreach (var childId in treeNodeViewModel.Children)
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
            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        /// <summary>
        /// 折叠节点
        /// </summary>
        /// <param name="treeNodeViewModel"></param>
        public void Fold(TreeNodeViewModel treeNodeViewModel)
        {
            List<int> allChildId = new List<int>();
            this.GetAllChildrenId(treeNodeViewModel, allChildId);

            foreach (int childId in allChildId)
            {
                TreeNodeViewModel child = this.Get(childId);
                this.treeNodes.Remove(child);
            }

            treeNodeViewModel.IsFold = true;

            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        /// <summary>
        /// 展开节点,一级一级展开,一次只展开下层子节点,比如下层节点是折叠的,那下下层节点不展开
        /// </summary>
        /// <param name="treeNodeViewModel"></param>
        public void UnFold(TreeNodeViewModel treeNodeViewModel)
        {
            treeNodeViewModel.IsFold = false;

            List<int> allChildId = new List<int>();
            this.GetAllChildrenId(treeNodeViewModel, allChildId);

            foreach (int childId in allChildId)
            {
                TreeNodeViewModel child = this.Get(childId);
                this.treeNodes.Add(child);
            }

            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        public void MoveLeft(TreeNodeViewModel treeNodeViewModel)
        {
            if (treeNodeViewModel.IsRoot)
            {
                return;
            }
            var parent = treeNodeViewModel.Parent;
            int index = parent.Children.IndexOf(treeNodeViewModel.Id);
            if (index == 0)
            {
                return;
            }
            parent.Children.Remove(treeNodeViewModel.Id);
            parent.Children.Insert(index - 1, treeNodeViewModel.Id);

            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }

        public void MoveRight(TreeNodeViewModel treeNodeViewModel)
        {
            if (treeNodeViewModel.IsRoot)
            {
                return;
            }
            var parent = treeNodeViewModel.Parent;
            int index = parent.Children.IndexOf(treeNodeViewModel.Id);
            if (index == parent.Children.Count - 1)
            {
                return;
            }
            parent.Children.Remove(treeNodeViewModel.Id);
            parent.Children.Insert(index + 1, treeNodeViewModel.Id);

            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout();
        }
    }
}