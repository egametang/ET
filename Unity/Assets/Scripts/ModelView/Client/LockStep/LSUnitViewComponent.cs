namespace ET
{
	[ComponentOf(typeof(Room))]
	public class LSUnitViewComponent: Entity, IAwake, IDestroy
	{
		public EntityRef<LSUnitView> myUnitView;
	}
}