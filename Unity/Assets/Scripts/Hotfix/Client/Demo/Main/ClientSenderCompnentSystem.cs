namespace ET.Client
{
    [FriendOf(typeof(ClientSenderCompnent))]
    public static partial class NetClientProxyCompnentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSenderCompnent self)
        {
            self.ActorSender = self.Root().GetComponent<ActorSenderComponent>();
        }
        
        [EntitySystem]
        private static void Destroy(this ClientSenderCompnent self)
        {
            if (self.fiberId != 0)
            {
                FiberManager.Instance.Remove(self.fiberId);
            }
        }
        
        public static async ETTask<long> LoginAsync(this ClientSenderCompnent self, string account, string password)
        {
            self.fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, 0, SceneType.NetClient, "");
            self.netClientActorId = new ActorId(self.Fiber().Process, self.fiberId);
            
            NetClient2Main_Login response =await self.ActorSender.Call(self.netClientActorId, new Main2NetClient_Login() {Account = account, Password = password}) as NetClient2Main_Login;
            return response.PlayerId;
        }
        
        public static void Send(this ClientSenderCompnent self, IMessage message)
        {
            A2NetClient_Message a2NetClientMessage = A2NetClient_Message.Create();
            a2NetClientMessage.MessageObject = message;
            self.ActorSender.Send(self.netClientActorId, a2NetClientMessage);
        }
        
        public static async ETTask<IResponse> Call(this ClientSenderCompnent self, IRequest request, bool needException = true)
        {
            A2NetClient_Request a2NetClientRequest = A2NetClient_Request.Create();
            a2NetClientRequest.MessageObject = request;
            A2NetClient_Response response = await self.ActorSender.Call(self.netClientActorId, a2NetClientRequest, needException) as A2NetClient_Response;
            return response.MessageObject;
        }
    }
}