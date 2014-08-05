using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Modules.Tree
{
    [DataContract]
    public class TreeNodeData
    {
        private readonly List<int> children = new List<int>();

        /// <summary>
        /// 节点唯一Id
        /// </summary>
        [DataMember(Order = 1)]
        public int Id { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        [DataMember(Order = 2)]
        public int Type { get; set; }

        /// <summary>
        /// 节点配置参数
        /// </summary>
        [DataMember(Order = 3)]
        public List<string> Args { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        [DataMember(Order = 4)]
        public int Parent { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        [DataMember(Order = 5)]
        public List<int> Children
        {
            get
            {
                return this.children;
            }
        }

        /// <summary>
        /// 该节点属于哪颗树
        /// </summary>
        [DataMember(Order = 6)]
        public int TreeId { get; set; }

        /// <summary>
        /// 节点说明
        /// </summary>
        [DataMember(Order = 7)]
        public string Comment { get; set; }
    }
}