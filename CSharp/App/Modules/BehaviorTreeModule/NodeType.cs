namespace Modules.BehaviorTreeModule
{
	public enum NodeType
	{
		Selector = 1,
		Sequence = 2,
		Not = 10,
		Weight = 11,
		True = 12,

		MoveToTarget = 100,
		Desdroy = 101,
		DistanceWithTarget = 102,
	}
}