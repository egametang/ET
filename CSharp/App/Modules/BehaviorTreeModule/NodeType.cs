namespace Modules.BehaviorTreeModule
{
	public enum NodeType
	{
		Selector = 1,
		Sequence = 2,
		Not = 10,
		Weight = 11,
		True = 12,

		// condition节点 10000开始
		SelectTarget = 10000,
		Roll = 10001,
		Compare = 10002,
		BulletDistance = 10003,
		OwnBuff = 10004,
		FriendDieInDistance = 10005,
		LessHp = 10007,
		InGlobalCD = 10008,
		TargetDie = 100010,
		TargetDistance = 100011,
		UnitState = 100012,
		// 与主角的距离
		ProtagonistDistance = 100013,
		OwnEffect = 100014,
		// 属性比较
		NumbericCompare = 100015,
		// 连续受击数
		ContinuousBeHitted = 100016,
		// 携带的技能是否匹配前缀
		HaveSpellWithPrefix = 100017,
		// 匹配前缀的技能是否在cd中
		SpellPrefixInCD = 100018,
		// 开启了跟随模式
		EnableFollow = 100019,
		// 方形陷阱选择目标
		RectTrapSelectTarget = 100020,



		// action节点 20000开始
		CastSpell = 20000,
		Chase = 20001,
		Empty = 20002,
		Patrol = 20003,
		Idle = 20004,
		CastDefaultSpell = 20005,
		Destroy = 20006,
		Move = 20007,
		CloseTarget = 20008,
		LeaveTarget = 20009,
		PathPatrol = 20010,
		CastPrefixSpell = 20011,
		// 跟随unit
		FollowUnit = 20012,
		LookAtTarget = 20013,
		CastNumSpell = 20014,
	}
}