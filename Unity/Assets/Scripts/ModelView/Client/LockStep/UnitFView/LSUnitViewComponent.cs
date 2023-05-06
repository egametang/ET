namespace ET
{
	[ComponentOf(typeof(Room))]
	public class LSUnitViewComponent: Entity, IAwake, IDestroy, IUpdate, IRollback
	{
		public long MyId;
	}
}