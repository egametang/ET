namespace ET.Client
{
    [EntitySystemOf(typeof(ClientSenderCompnent))]
    [FriendOf(typeof(ClientSenderCompnent))]
    public static partial class ClientSenderCompnentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSenderCompnent self)
        {

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

            NetClient2Main_Login response = await self.Fiber().ActorInnerComponent.Call(self.netClientActorId, new Main2NetClient_Login() { Account = account, Password = password }) as NetClient2Main_Login;
            return response.PlayerId;
        }

        public static void Send(this ClientSenderCompnent self, IMessage message)
        {
            A2NetClient_Message a2NetClientMessage = A2NetClient_Message.Create();
            a2NetClientMessage.MessageObject = message;
            self.Fiber().ActorInnerComponent.Send(self.netClientActorId, a2NetClientMessage);
        }

        public static async ETTask<IResponse> Call(this ClientSenderCompnent self, IRequest request, bool needException = true)
        {
            A2NetClient_Request a2NetClientRequest = A2NetClient_Request.Create();
            a2NetClientRequest.MessageObject = request;
            A2NetClient_Response response = await self.Fiber().ActorInnerComponent.Call(self.netClientActorId, a2NetClientRequest, needException: needException) as A2NetClient_Response;
            return response.MessageObject;
        }

    }
}