namespace ET.Server
{
    public static class Entry
    {
        public static void Start()
        {
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            await Game.EventSystem.Callback<ETTask>(CallbackType.InitShare);
            await Game.EventSystem.Callback<ETTask>(CallbackType.InitServer);
        }
    }
}