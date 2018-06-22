using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Login)]
    public class G2L_CreateCharacter_Handler : AMRpcHandler<G2L_CreateCharacter_Request, L2G_CreateCharacter_Response>
    {
        protected override async void Run(Session session, G2L_CreateCharacter_Request message, Action<L2G_CreateCharacter_Response> reply)
        {
            L2G_CreateCharacter_Response response = new L2G_CreateCharacter_Response();
            try
            {
                //检测角色是否在限定范围
                if (!GameTool.ContainCharacterId(message.CharacterID))
                {
                    //todo 停权处理
                    response.Error = ErrorCode.ERR_Cheat;
                    reply(response);
                    return;
                }

                //名字合法字符检测
                if (!GameTool.CharacterDetection(message.PlayerName) || message.PlayerName.Length > 8 || string.IsNullOrEmpty(message.PlayerName))
                {
                    //todo 停权处理
                    response.Error = ErrorCode.ERR_Cheat;
                    reply(response);
                    return;
                }

                //点数作弊检测
                int totalstatepoint = message.HealthBP + message.StrBp + message.DefBP + message.SpeedBP + message.MagicBP;
                if (totalstatepoint < 30 || totalstatepoint > 30 || message.HealthBP < 0 || message.HealthBP > 15 || message.StrBp < 0 || message.StrBp > 15 ||
                    message.DefBP < 0 || message.DefBP > 15 || message.SpeedBP < 0 || message.SpeedBP > 15 || message.MagicBP < 0 || message.MagicBP > 15)
                {
                    //todo 停权处理
                    response.Error = ErrorCode.ERR_Cheat;
                    reply(response);
                    return;
                }

                //水晶作弊检测
                int coutter = 0;
                int dimondtype1 = -1;
                int dimondtype2 = -1;
                int dimondvalue1 = 0;
                int dimondvalue2 = 0;
                if (message.Di > 0)
                {
                    coutter++;
                    if (dimondtype1 < 0)
                    {
                        dimondtype1 = 1;
                        dimondvalue1 = message.Di * 10;
                    }
                    else
                    {
                        dimondtype2 = 1;
                        dimondvalue2 = message.Di * 10;
                    }
                }

                if (message.Shui > 0)
                {
                    coutter++;
                    if (dimondtype1 < 0)
                    {
                        dimondtype1 = 2;
                        dimondvalue1 = message.Shui * 10;
                    }
                    else
                    {
                        dimondtype2 = 2;
                        dimondvalue2 = message.Shui * 10;
                    }
                }

                if (message.Huo > 0)
                {
                    coutter++;
                    if (dimondtype1 < 0)
                    {
                        dimondtype1 = 3;
                        dimondvalue1 = message.Huo * 10;
                    }
                    else
                    {
                        dimondtype2 = 3;
                        dimondvalue2 = message.Huo * 10;
                    }
                }

                if (message.Feng > 0)
                {
                    coutter++;
                    if (dimondtype1 < 0)
                    {
                        dimondtype1 = 4;
                        dimondvalue1 = message.Feng * 10;
                    }
                    else
                    {
                        dimondtype2 = 4;
                        dimondvalue2 = message.Feng * 10;
                    }
                }
                int totaldimondpoint = message.Di + message.Shui + message.Huo + message.Feng;
                if (coutter > 2 || totaldimondpoint < 10 && totaldimondpoint > 10 || message.Di < 0 || message.Shui < 0 || message.Huo < 0 || message.Feng < 0 ||
                    (message.Di > 0 && message.Huo > 0) || (message.Shui > 0 && message.Feng > 0))
                {
                    //todo 停权处理
                    response.Error = ErrorCode.ERR_Cheat;
                    reply(response);
                    return;
                }

                //开始创建新角色:
                Character info = new Character
                {
                    Id = message.UserID,
                    Role = new Role
                    {
                        Name = message.PlayerName,
                        CharacterID = message.CharacterID,
                        Level = 1,
                        JobLevel = 1,
                        CardTimeHour = 1,
                        MapID = "1530",
                        Dong = 15,
                        Nan = 6,
                        HealthBP = message.HealthBP,
                        StrBP = message.StrBp,
                        DefBP = message.DefBP,
                        SpeedBP = message.SpeedBP,
                        MagicBP = message.MagicBP,
                        LockCoin = 500,
                        Meili = 60,
                        Naili = 50,
                        Zhili = 50,
                        Lingqiao = 50,
                        MaxItemCount = 20,
                        MaxPetCount = 5,
                        MaxSkillCount = 10,
                        Fram = 1000,
                        FramRestrict = 800,
                        SecletTitleId = 1, //游民
                        LastEatTime = DateTime.Now,
                        DressId = new List<int> { message.CharacterID },
                    },
                    //创造水晶在身上
                    Equip = new List<Item>
                    {
                        new Item
                        {
                            EqInfo = new Equipable
                            {
                                ShuxingType1 = dimondtype1,
                                ShuxingType2 = dimondtype2,
                                ShuxingValue1 = dimondvalue1,
                                ShuxingValue2 = dimondvalue2,
                            },
                            SlotID = 6,
                            Count = 1,
                            ItemID = 9200,
                            ItemType = 22,
                            IsDropDispare = true,
                            CanUse = true,
                            OverlayCount = 1,
                            ItemLevel = 1,
                            UsageNow = 1000,
                            UsageTotal = 1000,
                            Guid = IdGenerater.GenerateId() + "",
                        }
                    },
                    Title = new List<Title> //0=系统 1=职业 3=家族 4=额外 5=装备 6=自定义
                    {
                        new Title {TitleType = 0, TitleId = 4},
                        new Title {TitleType = 1, TitleId = 1},
                    }
                };
                info.Role.HPNow = 20 + (info.Role.HealthBP * 8) + (info.Role.StrBP * 2) + (info.Role.DefBP * 3) + (info.Role.SpeedBP * 3) + (info.Role.MagicBP * 1);
                info.Role.MPNow = 20 + (info.Role.HealthBP * 1) + (info.Role.StrBP * 2) + (info.Role.DefBP * 2) + (info.Role.SpeedBP * 2) + (info.Role.MagicBP * 10);

                StartConfig dbStartConfig = Game.Scene.GetComponent<StartConfigComponent>().DBConfig;
                Session dbSession = Game.Scene.GetComponent<NetInnerComponent>().Get(dbStartConfig.GetComponent<InnerConfig>().IPEndPoint);
                D2L_CreateCharacter_Response dbresonse = await dbSession.Call(new L2D_CreateCharacter_Request {CollectionName = "Character", Info = info, NeedCache = true}) as D2L_CreateCharacter_Response;

                //名字已重复
                if (dbresonse.Error == ErrorCode.ERR_CharacterNameAlreadyExist)
                {
                    response.Error = dbresonse.Error;
                    response.Info = null;
                    reply(response);
                    return;
                }

                //建立成功
                if (dbresonse.Error == ErrorCode.ERR_Success)
                {
                    Log.Info($"新角色创建成功{MongoHelper.ToJson(info)}");
                    response.Error = dbresonse.Error;
                    response.Info = RoleDataConvertHelper.GetBasicRoleInfo(info);
                    reply(response);
                    return;
                }

                Log.Info($"新角色创建出现未知异常{MongoHelper.ToJson(info)} ErrorCode: " + dbresonse.Error);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}