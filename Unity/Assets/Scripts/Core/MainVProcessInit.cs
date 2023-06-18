namespace ET
{
    public static class MainVProcessInit
    {
        public static void Init(VProcess vProcess)
        {
            vProcess.AddSingleton<Root>();
        }
    }
}