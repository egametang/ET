using System;

namespace ET.Server
{
    [Invoke((long)SceneType.BenchmarkClient)]
    public class NetComponentOnReadInvoker_BenchmarkClient: AInvokeHandler<NetComponentOnRead>
    {
        public override void Handle(NetComponentOnRead args)
        {
            Session session = args.Session;
            object message = args.Message;
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message)
            {
                case IResponse response:
                {
                    session.OnResponse(response);
                    break;
                }
            }
        }
    }
}