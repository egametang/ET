namespace DotRecast.Detour.Extras.Jumplink
{
    public class JumpLinkBuilderConfig
    {
        public readonly float cellSize;
        public readonly float cellHeight;
        public readonly float agentClimb;
        public readonly float agentRadius;
        public readonly float groundTolerance;
        public readonly float agentHeight;
        public readonly float startDistance;
        public readonly float endDistance;
        public readonly float jumpHeight;
        public readonly float minHeight;
        public readonly float heightRange;

        public JumpLinkBuilderConfig(float cellSize, float cellHeight, float agentRadius, float agentHeight,
            float agentClimb, float groundTolerance, float startDistance, float endDistance, float minHeight,
            float maxHeight, float jumpHeight)
        {
            this.cellSize = cellSize;
            this.cellHeight = cellHeight;
            this.agentRadius = agentRadius;
            this.agentClimb = agentClimb;
            this.groundTolerance = groundTolerance;
            this.agentHeight = agentHeight;
            this.startDistance = startDistance;
            this.endDistance = endDistance;
            this.minHeight = minHeight;
            heightRange = maxHeight - minHeight;
            this.jumpHeight = jumpHeight;
        }
    }
}