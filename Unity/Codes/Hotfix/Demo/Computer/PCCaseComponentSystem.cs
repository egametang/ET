namespace ET
{
    public static class PCCaseComponentSystem
    {
        public static void StartPCCase(this PCCaseComponent self)
        {
            Log.Debug(self.ToString());
            Log.Debug("Start PCCase ! ! !");
        }
    }
}