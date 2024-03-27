using System.Threading.Tasks;

namespace ET.Client
{
    [EntitySystemOf(typeof(ClientSenderComponent))]
    [FriendOf(typeof(ClientSenderComponent))]
    public static partial class ClientSenderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSenderComponent self)
        {
            
        }

        public static async ETTask OnAwake(this ClientSenderComponent self)
        {
            //声明一个纤程，来自池
            self.fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, 0, SceneType.NetClient, "");
            //声明一个ActorId
            self.netClientActorId = new ActorId(self.Fiber().Process, self.fiberId);
        }

        public static ActorId GetActorId(this ClientSenderComponent self) => self.netClientActorId;
        
        [EntitySystem]
        private static void Destroy(this ClientSenderComponent self)
        {
            self.RemoveFiberAsync().Coroutine();
        }

        private static async ETTask RemoveFiberAsync(this ClientSenderComponent self)
        {
            if (self.fiberId == 0)
            {
                return;
            }

            int fiberId = self.fiberId;
            self.fiberId = 0;
            await FiberManager.Instance.Remove(fiberId);
        }

        public static async ETTask DisposeAsync(this ClientSenderComponent self)
        {
            await self.RemoveFiberAsync();
            self.Dispose();
        }

        /// <summary>
        /// 只发不收
        /// </summary>
        /// <param name="self"></param>
        /// <param name="message"></param>
        public static void Send(this ClientSenderComponent self, IMessage message)
        {
            A2NetClient_Message a2NetClientMessage = A2NetClient_Message.Create();
            a2NetClientMessage.MessageObject = message;
            self.Root().GetComponent<ProcessInnerSender>().Send(self.netClientActorId, a2NetClientMessage);
        }

        /// <summary>
        /// 外部发了还收
        /// </summary>
        public static async ETTask<IResponse> Call(this ClientSenderComponent self, IRequest request, bool needException = true)
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