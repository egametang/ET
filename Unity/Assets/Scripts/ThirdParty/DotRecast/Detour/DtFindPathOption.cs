namespace DotRecast.Detour
{
    public readonly struct DtFindPathOption
    {
        public static readonly DtFindPathOption NoOption = new DtFindPathOption(DefaultQueryHeuristic.Default, 0, 0);

        public static readonly DtFindPathOption AnyAngle = new DtFindPathOption(DefaultQueryHeuristic.Default, DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE, float.MaxValue);
        public static readonly DtFindPathOption ZeroScale = new DtFindPathOption(new DefaultQueryHeuristic(0.0f), 0, 0);

        public readonly IQueryHeuristic heuristic;
        public readonly int options;
        public readonly float raycastLimit;

        public DtFindPathOption(IQueryHeuristic heuristic, int options, float raycastLimit)
        {
            this.heuristic = heuristic;
            this.options = options;
            this.raycastLimit = raycastLimit;
        }

        public DtFindPathOption(int options, float raycastLimit)
            : this(DefaultQueryHeuristic.Default, options, raycastLimit)
        {
        }
    }
}