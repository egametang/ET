using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.DB)]
    public class L2D_CrateCharacter_Handler : AMRpcHandler<L2D_CreateCharacter_Request, D2L_CreateCharacter_Response>
    {
        protected override async void Run(Session session, L2D_CreateCharacter_Request message, Action<D2L_CreateCharacter_Response> reply)
        {
            D2L_CreateCharacter_Response response = new D2L_CreateCharacter_Response();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();

                //查询角色是否已建立
                var component = await dbCacheComponent.Get(message.CollectionName, message.Info.Id);
                if (component != null)
                {
                    response.Error = ErrorCode.ERR_Exception;
                    reply(response);
                    return;
                }

                //查询名字是否重复
                List<ComponentWithId> components = await dbCacheComponent.GetJson(message.CollectionName, "{'Role.Name':'" + message.Info.Role.Name + "'}");
                if (components.Count > 0)
                {
                    response.Error = ErrorCode.ERR_CharacterNameAlreadyExist;
                    reply(response);
                    return;
                }

                //开始建立新角色
                if (message.NeedCache) //是否需要缓存
                {
                    dbCacheComponent.AddToCache(message.Info, message.CollectionName);
                }
                await dbCacheComponent.Add(message.Info, message.CollectionName);

                Log.Debug("DB建立成功");
                response.Error = ErrorCode.ERR_Success;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}