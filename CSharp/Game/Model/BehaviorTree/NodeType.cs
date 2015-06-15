namespace Model
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

		/// <summary>
		/// 后退移动
		/// </summary>
		BackMove = 103,

		/// <summary>
		/// 平行移动
		/// </summary>
		MoveToTargetParallel = 104,

		PlayMotion = 210,

		/// <summary>
		/// 隐身
		/// </summary>
		ApplyCloak = 211,

		/// <summary>
		/// 取消隐身
		/// </summary>
		CancelCloak = 212,

		/// <summary>
		/// 改变模型
		/// </summary>
		ChangeModel = 213,

		/// <summary>
		/// 还原模型
		/// </summary>
		RevertModel = 214,

		/// <summary>
		/// 改变材质
		/// </summary>
		ChangeMaterial = 215,

		/// <summary>
		/// 还原材质
		/// </summary>
		RevertMaterial = 216,

		/// <summary>
		/// 飞上天
		/// </summary>
		Fly = 217,

		/// <summary>
		/// 从天上落地
		/// </summary>
		FlyFinish = 218,

		CreateEffect = 219,

		CreateBuilding = 220,

		/// <summary>
		/// 巨型技能指示器
		/// </summary>
		RectSkillIndicator = 221,

		/// <summary>
		/// 圆形技能指示器
		/// </summary>
		RoundSkillIndicator = 222,
		/// <summary>
		/// 扇形指示器
		/// </summary>
		SectorSkillIndicator = 223,
		/// <summary>
		/// 连线特效
		/// </summary>
		LinkEffect = 224,
		/// <summary>
		/// 普通特效
		/// </summary>
		EffectCommond = 225,
	}
}