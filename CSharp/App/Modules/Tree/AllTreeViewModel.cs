using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using Helper;

namespace Tree
{
    [Export(contractType: typeof (AllTreeViewModel)),
     PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
    public class AllTreeViewModel
    {
        private AllTreeData allTreeData;

        public int MaxNodeId { get; set; }
        public int MaxTreeId { get; set; }

        public Dictionary<int, List<TreeNodeData>> treeDict = new Dictionary<int, List<TreeNodeData>>();

        private readonly Dictionary<int, TreeViewModel> treeViewModelsDict = new Dictionary<int, TreeViewModel>(); 

        public ObservableCollection<TreeInfoViewModel> treeList = new ObservableCollection<TreeInfoViewModel>();

        public ObservableCollection<TreeInfoViewModel> TreeList
        {
            get
            {
                return this.treeList;
            }
        }

        public void Load(string file)
        {
            treeDict.Clear();
            treeList.Clear();
            byte[] bytes = File.ReadAllBytes(file);
            this.allTreeData = ProtobufHelper.FromBytes<AllTreeData>(bytes);

            this.MaxNodeId = 0;
            this.MaxTreeId = 0;
            foreach (TreeNodeData treeNodeData in allTreeData.TreeNodeDatas)
            {
                List<TreeNodeData> tree;
                this.treeDict.TryGetValue(treeNodeData.TreeId, out tree);
                if (tree == null)
                {
                    tree = new List<TreeNodeData>();
                    this.treeDict[treeNodeData.TreeId] = tree;
                }
                tree.Add(treeNodeData);
                if (treeNodeData.Id > this.MaxNodeId)
                {
                    this.MaxNodeId = treeNodeData.Id;
                }
                if (treeNodeData.TreeId > this.MaxTreeId)
                {
                    this.MaxTreeId = treeNodeData.TreeId;
                }

                treeList.Add(new TreeInfoViewModel(treeNodeData.TreeId, treeNodeData.TreeId.ToString()));
            }
        }

        public void Save(string file)
        {
            
        }

        public void New(TreeViewModel treeViewModel)
        {
        }

        public void Add(TreeViewModel treeViewModel)
        {
            treeViewModel.TreeId = ++this.MaxTreeId;
            treeViewModel.AllTreeViewModel = this;
            treeDict[treeViewModel.TreeId] = treeViewModel.TreeNodeDatas;
            treeViewModelsDict[treeViewModel.TreeId] = treeViewModel;
            this.treeList.Add(new TreeInfoViewModel(treeViewModel.TreeId, treeViewModel.TreeId.ToString()));
        }

        public TreeViewModel Get(int treeId)
        {
            if (this.treeViewModelsDict.ContainsKey(treeId))
            {
                return this.treeViewModelsDict[treeId];
            }
            var treeViewModel = new TreeViewModel(this.treeDict[treeId]);
            this.treeViewModelsDict[treeId] = treeViewModel;
            return treeViewModel;
        }
    }
}