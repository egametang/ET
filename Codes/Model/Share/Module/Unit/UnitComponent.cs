namespace ET
{
	[ChildType(typeof(Unit))]
	[ComponentOf(typeof(Scene))]
	public class UnitComponent: Entity, IAwake, IDestroy
	{
	}
}