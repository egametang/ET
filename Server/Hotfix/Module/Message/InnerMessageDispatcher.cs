using ETModel;

namespace ETHotfix
{
    public class InnerMessageDispatcher: IMessageDispatcher
    {
        public void Dispatch(Session session, ushort opcode, object message)
        {
            // 收到actor消息,放入actor队列
            switch (message)
            {
                case IActorRequest iActorRequest:
                {
                    InnerMessageDispatcherHelper.HandleIActorRequest(session, iActorRequest).Coroutine();
                    return;
                }
                case IActorMessage iactorMessage:
                {
                    InnerMessageDispatcherHelper.HandleIActorMessage(session, iactorMessage).Coroutine();
                    return;
                }
                case IActorResponse iActorResponse:
                {
                    InnerMessageDispatcherHelper.HandleIActorResponse(session, iActorResponse).Coroutine();
                    return;
                }
                default:
                {
                    MessageDispatcherComponent.Instace.Handle(session, new MessageInfo(opcode, message));
                    break;
                }
            }
        }
		

    }
}