namespace DotRecast.Detour
{
    public readonly struct DtSegInterval
    {
        public readonly long refs;
        public readonly int tmin;
        public readonly int tmax;

        public DtSegInterval(long refs, int tmin, int tmax)
        {
            this.refs = refs;
            this.tmin = tmin;
            this.tmax = tmax;
        }
    }
}