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

		PlayMotion = 210,
		ApplyCloak = 211,
		CancelCloak = 212,
		ChangeModel = 213,
		RevertModel = 214,
		ChangeMaterial = 215,
		RevertMaterial = 216,
		CreateEffect = 219,
	}
}