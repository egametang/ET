namespace ET.Server
{
    public struct RobotCallbackArgs: ICallback
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }
}