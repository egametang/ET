using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Gate)]
    public class GetUserInfoRtHandler : AMRpcHandler<GetUserInfoRt, GetUserInfoRe>
    {
        protected override async void Run(Session session, GetUserInfoRt message, Action<GetUserInfoRe> reply)
        {
            GetUserInfoRe response = new GetUserInfoRe();
            try
            {
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();

                //查询用户信息
                UserInfo userInfo = await dbProxy.Query<UserInfo>(message.UserId);

                if (userInfo == null)
                {
                    response.Error = ErrorCode.ErrQueryUserInfoError;
                    reply(response);
                    return;
                }

                response.NickName = userInfo.NickName;
                response.Wins = userInfo.Wins;
                response.Loses = userInfo.Loses;
                response.Money = userInfo.Money;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
