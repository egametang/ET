namespace DotRecast.Detour.Extras.Jumplink
{
    public class JumpLink
    {
        public const int MAX_SPINE = 8;
        public readonly int nspine = MAX_SPINE;
        public readonly float[] spine0 = new float[MAX_SPINE * 3];
        public readonly float[] spine1 = new float[MAX_SPINE * 3];
        public GroundSample[] startSamples;
        public GroundSample[] endSamples;
        public GroundSegment start;
        public GroundSegment end;
        public Trajectory trajectory;
    }
}