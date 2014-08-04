using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Mvvm;

namespace Tree
{
    [Export(typeof (TreeViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class TreeViewModel: BindableBase
    {
        private readonly ObservableCollection<TreeNodeViewModel> treeNodes =
                new ObservableCollection<TreeNodeViewModel>();

        private readonly Dictionary<int, TreeNodeViewModel> treeNodeDict = new Dictionary<int, TreeNodeViewModel>(); 

        public ObservableCollection<TreeNodeViewModel> TreeNodes
        {
            get
            {
                return this.treeNodes;
            }
        }

        private TreeNodeViewModel Root
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
            if (parent != null && parent.IsFolder)
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
            treeLayout.ExcuteLayout(this.Root);
        }

        private void RecursionRemove(TreeNodeViewModel treeNodeViewModel)
        {
            for (int i = 0; i < treeNodeViewModel.Children.Count; ++i)
            {
                TreeNodeViewModel child = this.Get(treeNodeViewModel.Children[i]);
                this.RecursionRemove(child);
            }
            this.treeNodeDict.Remove(treeNodeViewModel.Id);
            this.treeNodes.Remove(treeNodeViewModel);
        }

        public void Remove(TreeNodeViewModel treeNodeViewModel)
        {
            this.RecursionRemove(treeNodeViewModel);
            if (treeNodeViewModel.Parent != null)
            {
                treeNodeViewModel.Parent.Children.Remove(treeNodeViewModel.Id);
            }
            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout(this.Root);
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

            if (from.IsFolder)
            {
                this.UnFold(from);
            }

            if (to.IsFolder)
            {
                this.UnFold(to);
            }
            from.Parent.Children.Remove(from.Id);
            to.Children.Add(from.Id);
            from.Parent = to;
            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout(this.Root);
        }

        /// <summary>
        /// 折叠节点
        /// </summary>
        /// <param name="treeNodeViewModel"></param>
        public void Fold(TreeNodeViewModel treeNodeViewModel)
        {
            foreach (var childId in treeNodeViewModel.Children)
            {
                TreeNodeViewModel child = this.Get(childId);
                this.RecursionRemove(child);
            }
            treeNodeViewModel.IsFolder = true;
            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout(this.Root);
        }

        /// <summary>
        /// 展开节点,一级一级展开,一次只展开下层子节点,比如下层节点是折叠的,那下下层节点不展开
        /// </summary>
        /// <param name="unFoldNode"></param>
        public void UnFold(TreeNodeViewModel unFoldNode)
        {
            foreach (var childId in unFoldNode.Children)
            {
                TreeNodeViewModel child = this.Get(childId);
                this.RecursionAdd(child);
            }
            unFoldNode.IsFolder = false;
            var treeLayout = new TreeLayout(this);
            treeLayout.ExcuteLayout(this.Root);
        }

        private void RecursionAdd(TreeNodeViewModel treeNodeViewModel)
        {
            if (!this.treeNodes.Contains(treeNodeViewModel))
            {
                this.treeNodes.Add(treeNodeViewModel);
            }
            List<int> children = treeNodeViewModel.Children;

            if (treeNodeViewModel.IsFolder)
            {
                return;
            }
            foreach (var childId in children)
            {
                TreeNodeViewModel child = this.Get(childId);
                this.RecursionAdd(child);
            }
        }

        /// <summary>
        /// 序列化保存
        /// </summary>
        public void Save(string filePath)
        {
            //this.RecursionSave(treeNodeDataArray, this.Root);
            //byte[] bytes = ProtobufHelper.ToBytes(treeNodeDataArray);
            //using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            //{
            //    stream.Write(bytes, 0, bytes.Length);
            //}
        }

        private void RecursionSave(AllTreeData allTreeData, TreeNodeViewModel node)
        {
            //if (node == null)
            //{
            //    return;
            //}
            //allTreeData.Add(node.TreeNodeData);
            //foreach (TreeNodeViewModel childNode in node.Children)
            //{
            //    this.RecursionSave(allTreeData, childNode);
            //}
        }
    }
}