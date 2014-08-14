namespace BehaviorTree
{
    [NodeAttribute(NodeType.Not, typeof(Not))]
    public class Not: Node
    {
        public Not(NodeConfig config): base(config)
        {
        }

        public override bool Run(BlackBoard blackBoard)
        {
            return !this.children[0].Run(blackBoard);
        }
    }
}