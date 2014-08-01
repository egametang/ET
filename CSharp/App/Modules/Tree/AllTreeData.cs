using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tree
{
    [DataContract]
    public class AllTreeData
    {
        private readonly List<TreeNodeData> treeNodeDatas = new List<TreeNodeData>();
        public int MaxNodeId { get; set; }
        public int MaxTreeId { get; set; }

        [DataMember(Order = 1)]
        public List<TreeNodeData> TreeNodeDatas
        {
            get
            {
                return this.treeNodeDatas;
            }
        }

        private readonly Dictionary<int, TreeNodeData> allTreeNodes =
                new Dictionary<int, TreeNodeData>();

        /// <summary>
        /// tree对应的root id
        /// </summary>
        private readonly Dictionary<int, int> treeRootId = new Dictionary<int, int>();

        public void Init()
        {
            this.MaxNodeId = 0;
            this.MaxTreeId = 0;
            foreach (TreeNodeData nodeData in this.treeNodeDatas)
            {
                this.allTreeNodes[nodeData.Id] = nodeData;
                if (nodeData.Id > this.MaxNodeId)
                {
                    this.MaxNodeId = nodeData.Id;
                }
                if (nodeData.TreeId > this.MaxTreeId)
                {
                    this.MaxTreeId = nodeData.TreeId;
                }
                if (nodeData.Parent == 0)
                {
                    this.treeRootId[nodeData.TreeId] = nodeData.Id;
                }
            }
        }

        public void Save()
        {
            treeNodeDatas.Clear();
            foreach (KeyValuePair<int, TreeNodeData> pair in allTreeNodes)
            {
                treeNodeDatas.Add(pair.Value);
            }
        }

        /// <summary>
        /// 删除一棵树的所有节点
        /// </summary>
        /// <param name="treeId"></param>
        public void Remove(int treeId)
        {
            var removeList = new List<int>();
            foreach (int key in this.allTreeNodes.Keys)
            {
                TreeNodeData nodeData = allTreeNodes[key];
                if (nodeData.TreeId != treeId)
                {
                    continue;
                }
                removeList.Add(key);
            }

            foreach (int key in removeList)
            {
                this.allTreeNodes.Remove(key);
            }
            Save();
        }

        public TreeNodeData this[int id]
        {
            get
            {
                return this.allTreeNodes[id];
            }
        }
    }
}