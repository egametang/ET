namespace DotRecast.Detour.Crowd
{
    /// The type of navigation mesh polygon the agent is currently traversing.
    /// @ingroup crowd
    public enum DtCrowdAgentState
    {
        DT_CROWDAGENT_STATE_INVALID,

        /// < The agent is not in a valid state.
        DT_CROWDAGENT_STATE_WALKING,

        /// < The agent is traversing a normal navigation mesh polygon.
        DT_CROWDAGENT_STATE_OFFMESH, /// < The agent is traversing an off-mesh connection.
    };
}