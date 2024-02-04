using MemoryPack;

namespace ET
{
	[ComponentOf(typeof(LSWorld))]
	[MemoryPackable]
	public partial class LSUnitComponent: LSEntity, IAwake, ISerializeToEntity
	{
	}
}