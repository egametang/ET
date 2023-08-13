using System;

namespace ET.Server
{
    [Invoke((long)SceneType.Realm)]
    public class NetOuterComponentOnReadInvoker_Realm: AInvokeHandler<NetOuterComponentOnRead>
    {
        public override void Handle(NetOuterComponentOnRead args)
        {
            Session session = args.Session;
            object message = args.Message;
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message)
            {
                case ISessionMessage:
                {
                    MessageSessionDispatcher.Instance.Handle(session, message);
                    break;
                }
                default:
                {
                    throw new Exception($"not found handler: {message}");
                }
            }
        }
    }
}