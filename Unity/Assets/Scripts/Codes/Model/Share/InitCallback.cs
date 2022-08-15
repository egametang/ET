namespace ET
{
    [UniqueId(1,10000)]
    public static class InitCallbackId
    {
        public const int InitShare = 1;
        public const int InitClient = 2;
        public const int InitServer = 3;
    }
    
    public struct InitCallback: ICallback
    {
        public int Id { get; set; }
    }
}