using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tree
{
	[DataContract]
	public class TreeNodeData
	{
		private readonly List<int> childrenId = new List<int>();

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
		public int ParentId { get; set; }

		/// <summary>
		/// 子节点
		/// </summary>
		[DataMember(Order = 5)]
		public List<int> ChildrenId
		{
			get
			{
				return this.childrenId;
			}
		}

		/// <summary>
		/// 节点说明
		/// </summary>
		[DataMember(Order = 6)]
		public string Comment { get; set; }
	}
}