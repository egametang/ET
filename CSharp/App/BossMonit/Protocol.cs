using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BossMonit
{
	[DataContract]
	public class KVItem
	{
		[DataMember(Order = 1, IsRequired = true)]
		public byte[] Key { get; set; }

		[DataMember(Order = 2, IsRequired = false)]
		public byte[] Value { get; set; }
	}

	[DataContract]
	public class GmRequest
	{
		[DataMember(Order = 1, IsRequired = true)]
		public byte[] Cmd { get; set; }

		[DataMember(Order = 2, IsRequired = false)]
		public byte[] Param { get; set; }

		[DataMember(Order = 3)]
		public List<KVItem> ParamList { get; set; }
	}

	[DataContract]
	public class GmResult
	{
		[DataMember(Order = 1, IsRequired = true)]
		public int Result { get; set; }

		[DataMember(Order = 2, IsRequired = false)]
		public byte[] Data { get; set; }

		[DataMember(Order = 3)]
		public KVItem DataList { get; set; }
	}
}
