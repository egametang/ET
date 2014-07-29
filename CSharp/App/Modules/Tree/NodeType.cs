namespace Tree
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

        // action节点 20000开始
        CastSpell = 20000,
    }
}