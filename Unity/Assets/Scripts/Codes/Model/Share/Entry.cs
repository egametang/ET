namespace ET
{
    namespace EventType
    {
        public struct EntryEvent1
        {
        }   
        
        public struct EntryEvent2
        {
        } 
        
        public struct EntryEvent3
        {
        } 
    }
    
    public static class Entry
    {
        public static void Start()
        {
            MongoRegister.Init();
            
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            await EventSystem.Instance.PublishAsync(Game.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Game.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Game.Scene, new EventType.EntryEvent3());
        }
    }
}