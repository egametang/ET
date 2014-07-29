namespace BehaviorTree
{
    public class Selector: Node
    {
        public Selector(Config config)
        {
            this.Name = config.Name;
        }

        public override bool Run(BlackBoard blackBoard)
        {
            foreach (var child in this.children)
            {
                if (child.Run(blackBoard))
                {
                    return true;
                }
            }
            return false;
        }
    }
}