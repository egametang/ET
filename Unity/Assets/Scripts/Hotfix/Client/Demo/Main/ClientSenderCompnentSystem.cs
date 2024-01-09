using System.Threading.Tasks;

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
            self.RemoveFiberAsync().Coroutine();
        }

        private static async ETTask RemoveFiberAsync(this ClientSenderCompnent self)
        {
            if (self.fiberId == 0)
            {
                return;
            }

            int fiberId = self.fiberId;
            self.fiberId = 0;
            await FiberManager.Instance.Remove(fiberId);
        }

        public static async ETTask<long> LoginAsync(this ClientSenderCompnent self, string account, string password)
        {
            self.fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, 0, SceneType.NetClient, "");
            self.netClientActorId = new ActorId(self.Fiber().Process, self.fiberId);

            NetClient2Main_Login response = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, new Main2NetClient_Login()
            {
                OwnerFiberId = self.Fiber().Id, Account = account, Password = password
            }) as NetClient2Main_Login;
            return response.PlayerId;
        }

        public static void Send(this ClientSenderCompnent self, IMessage message)
        {
            A2NetClient_Message a2NetClientMessage = A2NetClient_Message.Create();
            a2NetClientMessage.MessageObject = message;
            self.Root().GetComponent<ProcessInnerSender>().Send(self.netClientActorId, a2NetClientMessage);
        }

        public static async ETTask<IResponse> Call(this ClientSenderCompnent self, IRequest request, bool needException = true)
        {
            A2NetClient_Request a2NetClientRequest = A2NetClient_Request.Create();
            a2NetClientRequest.MessageObject = request;
            using A2NetClient_Response a2NetClientResponse = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, a2NetClientRequest) as A2NetClient_Response;
            IResponse response = a2NetClientResponse.MessageObject;
                        
            if (response.Error == ErrorCore.ERR_MessageTimeout)
            {
                throw new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: {request}, response: {response}");
            }

            if (needException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                throw new RpcException(response.Error, $"Rpc error: {request}, response: {response}");
            }
            return response;
        }

    }
}