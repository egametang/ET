namespace ET
{
	[ComponentOf(typeof(BattleScene))]
	public class LSUnitViewComponent: Entity, IAwake, IDestroy, IUpdate
	{
		public long MyId;
	}
}