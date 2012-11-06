using System.Collections.Generic;
using ProtoBuf;

namespace Robot.Protos
{
	[ProtoContract]
	public class SMSG_Auth_Challenge
	{
		[ProtoMember(1, IsRequired = true)]
		public uint Num
		{
			get;
			set;
		}
		[ProtoMember(2, IsRequired = true)]
		public uint Seed
		{
			get;
			set;
		}
		[ProtoMember(3)]
		public List<uint> Random
		{
			get;
			set;
		}
	}
}
