namespace ET.Server
{
    public struct RobotInvokeArgs
    {
        public Fiber Fiber { get; set; }
        public string Content { get; set; }
    }
}