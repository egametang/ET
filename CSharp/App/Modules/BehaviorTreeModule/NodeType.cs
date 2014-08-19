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

        // action节点 20000开始
        CastSpell = 20000,
        Chase = 20001,
        Attack = 20002,
        Patrol = 20003,
    }
}