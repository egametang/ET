

using System;
using ETModel;

namespace ETHotfix
{
    // 用来测试消息包含复杂类型，是否产生gc
    [MessageHandler(AppType.AllServer)]

    public class TestLogin : AMRpcHandler<TestLoginRequest, TestLoginResponse>
    {
        protected override void Run(Session session, TestLoginRequest message, Action<TestLoginResponse> reply)
        {
            TestLoginResponse response = new TestLoginResponse();
            try {
                Console.WriteLine("userName" + message.UsreName);

                
                response.Message = "hahahaTestLogin,username:"+message.UsreName+"password:"+message.Password;
                reply(response);
            }
            catch (Exception ex) {
                ReplyError(response, ex, reply);
            }
            //throw new NotImplementedException();
        }
    }
}
