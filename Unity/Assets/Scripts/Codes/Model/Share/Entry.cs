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
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            MongoRegister.Init();
            
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}