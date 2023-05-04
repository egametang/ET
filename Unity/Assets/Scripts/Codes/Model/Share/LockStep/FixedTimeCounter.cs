namespace ET
{
    public class FixedTimeCounter
    {
        private long startTime;
        private long startFrame;
        public int Interval { get; private set; }

        public FixedTimeCounter(long startTime, long startFrame, int interval)
        {
            this.startTime = startTime;
            this.startFrame = startFrame;
            this.Interval = interval;
        }
        
        public void ChangeInterval(int interval, int frame)
        {
            this.startTime += (frame - this.startFrame) * this.Interval;
            this.startFrame = frame;
            this.Interval = interval;
        }

        public long FrameTime(int frame)
        {
            return this.startTime + (frame - this.startFrame) * this.Interval;
        }
    }
}