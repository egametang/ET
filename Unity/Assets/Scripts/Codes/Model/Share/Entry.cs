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
            MongoHelper.Register(Game.EventSystem.GetTypes());
            
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            await Game.EventSystem.PublishAsync(Game.Scene, new EventType.EntryEvent1());
            await Game.EventSystem.PublishAsync(Game.Scene, new EventType.EntryEvent2());
            await Game.EventSystem.PublishAsync(Game.Scene, new EventType.EntryEvent3());
        }
    }
}