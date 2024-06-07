namespace DotRecast.Detour.Crowd
{
    public class DtCrowdTimerLabel
    {
        public static readonly DtCrowdTimerLabel CheckPathValidity = new DtCrowdTimerLabel(nameof(CheckPathValidity));
        public static readonly DtCrowdTimerLabel UpdateMoveRequest = new DtCrowdTimerLabel(nameof(UpdateMoveRequest));
        public static readonly DtCrowdTimerLabel PathQueueUpdate = new DtCrowdTimerLabel(nameof(PathQueueUpdate));
        public static readonly DtCrowdTimerLabel UpdateTopologyOptimization = new DtCrowdTimerLabel(nameof(UpdateTopologyOptimization));
        public static readonly DtCrowdTimerLabel BuildProximityGrid = new DtCrowdTimerLabel(nameof(BuildProximityGrid));
        public static readonly DtCrowdTimerLabel BuildNeighbours = new DtCrowdTimerLabel(nameof(BuildNeighbours));
        public static readonly DtCrowdTimerLabel FindCorners = new DtCrowdTimerLabel(nameof(FindCorners));
        public static readonly DtCrowdTimerLabel TriggerOffMeshConnections = new DtCrowdTimerLabel(nameof(TriggerOffMeshConnections));
        public static readonly DtCrowdTimerLabel CalculateSteering = new DtCrowdTimerLabel(nameof(CalculateSteering));
        public static readonly DtCrowdTimerLabel PlanVelocity = new DtCrowdTimerLabel(nameof(PlanVelocity));
        public static readonly DtCrowdTimerLabel Integrate = new DtCrowdTimerLabel(nameof(Integrate));
        public static readonly DtCrowdTimerLabel HandleCollisions = new DtCrowdTimerLabel(nameof(HandleCollisions));
        public static readonly DtCrowdTimerLabel MoveAgents = new DtCrowdTimerLabel(nameof(MoveAgents));
        public static readonly DtCrowdTimerLabel UpdateOffMeshConnections = new DtCrowdTimerLabel(nameof(UpdateOffMeshConnections));

        public readonly string Label;

        private DtCrowdTimerLabel(string labelName)
        {
            Label = labelName;
        }
    }
}