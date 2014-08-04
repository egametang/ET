using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using Helper;

namespace Tree
{
    [Export(contractType: typeof (AllTreeViewModel)),
     PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
    internal class AllTreeViewModel
    {
        private AllTreeData allTreeData;

        public int MaxNodeId { get; set; }
        public int MaxTreeId { get; set; }

        private readonly ObservableCollection<int> treeList =
                new ObservableCollection<int>();

        public Dictionary<int, ObservableCollection<TreeNodeViewModel>> oneTree = 
            new Dictionary<int, ObservableCollection<TreeNodeViewModel>>();

        public ObservableCollection<int> TreeList
        {
            get
            {
                return this.treeList;
            }
        }

        public void Load(string file)
        {
            this.treeList.Clear();
            byte[] bytes = File.ReadAllBytes(file);
            this.allTreeData = ProtobufHelper.FromBytes<AllTreeData>(bytes);

            foreach (TreeNodeData treeNodeData in allTreeData.TreeNodeDatas)
            {
                ObservableCollection<TreeNodeViewModel> tree;
                this.oneTree.TryGetValue(treeNodeData.TreeId, out tree);
                if (tree == null)
                {
                    tree = new ObservableCollection<TreeNodeViewModel>();
                    oneTree[treeNodeData.TreeId] = tree;
                }
                //tree.Add(new TreeNodeViewModel(treeNodeData));
            }
        }

        public void Save(string file)
        {
            
        }
    }
}