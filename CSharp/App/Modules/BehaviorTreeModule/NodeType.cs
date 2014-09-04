namespace Modules.BehaviorTreeModule
{
    public enum NodeType
    {
        Selector = 1,
        Sequence = 2,
        Not = 10,

        // condition节点 10000开始
        SelectTarget = 10000,
        Roll = 10001,
        Compare = 10002,
        InAttackDistance = 10003,
        InChaseDistance = 10004,
        FriendDieInDistance = 10005,
        FriendLessHpInAttackDistance = 10006,
        LessHp = 10007,
        OnHit = 10008,
        SelfDie = 10009,
        TargetDie = 100010,
        TargetDistanceLess = 100011,

        // action节点 20000开始
        CastSpell = 20000,
        Chase = 20001,
        Attack = 20002,
        Patrol = 20003,
        Idle = 20004,
        SummonFriend = 20005,
        TalkToAll = 20006,
        CallFriends = 20007,
        CloseTarget = 20008,
        LeaveTarget = 20009,
    }
}