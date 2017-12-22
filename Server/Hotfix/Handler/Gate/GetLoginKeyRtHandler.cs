using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Gate)]
    public class GetLoginKeyRtHandler : AMRpcHandler<GetLoginKeyRt, GetLoginKeyRe>
    {
        protected override void Run(Session session, GetLoginKeyRt message, Action<GetLoginKeyRe> reply)
        {
            GetLoginKeyRe response = new GetLoginKeyRe();
            try
            {
                //随机密匙并添加到管理
                long key = RandomHelper.RandInt64();
                Game.Scene.GetComponent<GateSessionKeyComponent>().Add(key, message.UserId);

                response.Key = key;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
