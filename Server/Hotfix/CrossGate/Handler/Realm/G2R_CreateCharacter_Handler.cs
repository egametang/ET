using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class G2R_CreateCharacter_Handler : AMRpcHandler<C2G_CrateCharacter_Request, G2C_CrateCharacter_Response>
    {
        protected override async void Run(Session session, C2G_CrateCharacter_Request message, Action<G2C_CrateCharacter_Response> reply)
        {
            G2C_CrateCharacter_Response response = new G2C_CrateCharacter_Response();
            try
            {
                //验证Session
                if (!GateHelper.SignSession(session))
                {
                    response.Error = ErrorCode.ERR_SignError;
                    reply(response);
                    return;
                }


            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }

    }
}