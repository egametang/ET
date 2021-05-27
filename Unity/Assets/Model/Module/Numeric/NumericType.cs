namespace ET
{
    public enum NumericType
    {
		Max = 10000,

		Speed = 1000,
		SpeedBase = Speed * 10 + 1,
	    SpeedAdd = Speed * 10 + 2,
	    SpeedPct = Speed * 10 + 3,
	    SpeedFinalAdd = Speed * 10 + 4,
	    SpeedFinalPct = Speed * 10 + 5,

	    Hp = 1001,
	    HpBase = Hp * 10 + 1,

	    MaxHp = 1002,
	    MaxHpBase = MaxHp * 10 + 1,
	    MaxHpAdd = MaxHp * 10 + 2,
	    MaxHpPct = MaxHp * 10 + 3,
	    MaxHpFinalAdd = MaxHp * 10 + 4,
		MaxHpFinalPct = MaxHp * 10 + 5,
	}
}
