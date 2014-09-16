namespace Modules.BehaviorTreeModule
{
    public enum NodeType
    {
        Selector = 1,
        Sequence = 2,
        Not = 10,
        Weight = 11,

        // condition节点 10000开始
        SelectTarget = 10000,
        Roll = 10001,
        Compare = 10002,
        FriendDieInDistance = 10005,
        LessHp = 10007,
        TargetDie = 100010,
        TargetDistanceLess = 100011,
        UnitState = 100012,

        // action节点 20000开始
        CastSpell = 20000,
        Chase = 20001,
        Empty = 20002,
        Patrol = 20003,
        Idle = 20004,
        CloseTarget = 20008,
        LeaveTarget = 20009,
    }
}