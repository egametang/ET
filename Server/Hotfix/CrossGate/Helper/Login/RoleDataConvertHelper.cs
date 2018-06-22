using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETHotfix
{
    public static class RoleDataConvertHelper
    {
        public static LoginBasicRoleInfo GetBasicRoleInfo(Character cinfo)
        {
            if (cinfo != null)
            {
                RoleInfo roleinfo = new RoleInfo
                {
                    Name = cinfo.Role.Name,
                    VipDays = cinfo.Role.VipDays,
                    Level = cinfo.Role.Level,
                    Job = cinfo.Role.Job,
                    JobLevel = cinfo.Role.JobLevel,
                    Coin = cinfo.Role.Coin,
                    LockCoin = cinfo.Role.LockCoin,
                    CharacterID = cinfo.Role.CharacterID,
                    CustomTitle = cinfo.Role.CustomTitle,
                    SecletTitleId = cinfo.Role.SecletTitleId,
                    MapID = cinfo.Role.MapID,
                    Dong = cinfo.Role.Dong,
                    Nan = cinfo.Role.Nan,
                    NowExp = cinfo.Role.NowExp,
                    BPLeft = cinfo.Role.BPLeft,
                    HealthBP = cinfo.Role.HealthBP,
                    StrBP = cinfo.Role.StrBP,
                    DefBP = cinfo.Role.DefBP,
                    SpeedBP = cinfo.Role.SpeedBP,
                    MagicBP = cinfo.Role.MagicBP,
                    HealthState = cinfo.Role.HealthState,
                    IsFront = cinfo.Role.IsFront,
                    Meili = cinfo.Role.Meili,
                    Naili = cinfo.Role.Naili,
                    Zhili = cinfo.Role.Zhili,
                    Lingqiao = cinfo.Role.Lingqiao,
                };

                List<TitleInfo> titleinfo = null;
                if (cinfo.Title != null && cinfo.Title.Count > 0)
                {
                    titleinfo = cinfo.Title.Select(t => new TitleInfo { TitleID = t.TitleId, TitleType = t.TitleType }).ToList();
                }

                return new LoginBasicRoleInfo
                {
                    RoleInfo = roleinfo,
                    StateInfo = GetRoleStateInfo(cinfo),
                    TitleInfo = titleinfo
                };
            }

            return null;
        }

        public static RoleStateInfo GetRoleStateInfo(Character cinfo)
        {
            if (cinfo != null)
            {
                //灵魂加成
                float soul_attack = 1f;
                float soul_defend = 1f;
                float soul_speed = 1f;
                float soul_hp = 1f;
                float soul_mp = 1f;

                #region 装备总成数值

                int hp = 0;
                int mp = 0;
                int attack = 0;
                int defend = 0;
                int speed = 0;
                int jingshen = 0;
                int huifu = 0;
                int mogong = 0;
                int kangdu = 0;
                int kanghunshui = 0;
                int kangshihua = 0;
                int kangjiuzui = 0;
                int kanghunluan = 0;
                int kangyiwang = 0;
                int bisha = 0;
                int minzhong = 0;
                int fanji = 0;
                int di = 0;
                int shui = 0;
                int huo = 0;
                int feng = 0;

                //循环得到装备里的叠加信息
                if (cinfo.Equip != null && cinfo.Equip.Count > 0)
                {
                    for (int i = 0; i < cinfo.Equip.Count; i++)
                    {
                        //判断灵魂数值 todo

                        hp += cinfo.Equip[i].EqInfo.Hp;
                        mp += cinfo.Equip[i].EqInfo.Mp;
                        attack += cinfo.Equip[i].EqInfo.Attack;
                        defend += cinfo.Equip[i].EqInfo.Defend;
                        speed += cinfo.Equip[i].EqInfo.Speed;
                        jingshen += cinfo.Equip[i].EqInfo.Jingshen;
                        huifu += cinfo.Equip[i].EqInfo.Huifu;
                        mogong += cinfo.Equip[i].EqInfo.Mogong;
                        kangdu += cinfo.Equip[i].EqInfo.Kangdu;
                        kanghunshui += cinfo.Equip[i].EqInfo.Kanghunshui;
                        kangshihua += cinfo.Equip[i].EqInfo.Kangshihua;
                        kangjiuzui += cinfo.Equip[i].EqInfo.Kangjiuzui;
                        kanghunluan += cinfo.Equip[i].EqInfo.Kanghunluan;
                        kangyiwang += cinfo.Equip[i].EqInfo.Kangyiwang;
                        bisha += cinfo.Equip[i].EqInfo.Bisha;
                        minzhong += cinfo.Equip[i].EqInfo.Minzhong;
                        fanji += cinfo.Equip[i].EqInfo.Fanji;
                        di += cinfo.Equip[i].EqInfo.Di;
                        shui += cinfo.Equip[i].EqInfo.Shui;
                        huo += cinfo.Equip[i].EqInfo.Huo;
                        feng += cinfo.Equip[i].EqInfo.Feng;
                    }
                }

                #endregion

                RoleStateInfo info = new RoleStateInfo
                {
                    HPNow = cinfo.Role.HPNow,
                    HPTotal = (20 + (cinfo.Role.HealthBP * 8) + (cinfo.Role.StrBP * 2) + (cinfo.Role.DefBP * 3) + (cinfo.Role.SpeedBP * 3) + (cinfo.Role.MagicBP * 1) + hp) * soul_hp,
                    MPNow = cinfo.Role.MPNow,
                    MPTotal = (20 + (cinfo.Role.HealthBP * 1) + (cinfo.Role.StrBP * 2) + (cinfo.Role.DefBP * 2) + (cinfo.Role.SpeedBP * 2) + (cinfo.Role.MagicBP * 10) + mp) * soul_mp,
                    ZhongZhu = 0, //todo 种族
                    Attack = (20 + (cinfo.Role.HealthBP * 0.1f) + (cinfo.Role.StrBP * 2f) + (cinfo.Role.DefBP * 0.2f) + (cinfo.Role.SpeedBP * 0.2f) + (cinfo.Role.MagicBP * 0.1f) + attack) * soul_attack,
                    Defend = (20 + (cinfo.Role.HealthBP * 0.1f) + (cinfo.Role.StrBP * 0.2f) + (cinfo.Role.DefBP * 2f) + (cinfo.Role.SpeedBP * 0.2f) + (cinfo.Role.MagicBP * 0.1f) + defend) * soul_defend,
                    Speed = (20 + (cinfo.Role.HealthBP * 0.1f) + (cinfo.Role.StrBP * 0.2f) + (cinfo.Role.DefBP * 0.2f) + (cinfo.Role.SpeedBP * 2f) + (cinfo.Role.MagicBP * 0.1f) + speed) * soul_speed,
                    Jingshen = 100 + (cinfo.Role.HealthBP * -0.3f) + (cinfo.Role.StrBP * -0.1f) + (cinfo.Role.DefBP * 0.2f) + (cinfo.Role.SpeedBP * -0.1f) + (cinfo.Role.MagicBP * 0.8f) + jingshen,
                    Huifu = 100 + (cinfo.Role.HealthBP * 0.8f) + (cinfo.Role.StrBP * -0.1f) + (cinfo.Role.DefBP * -0.1f) + (cinfo.Role.SpeedBP * 0.2f) + (cinfo.Role.MagicBP * -0.3f) + huifu,
                    Kangdu = kangdu,
                    Kanghunshui = kanghunshui,
                    Kangshihua = kangshihua,
                    Kangjiuzui = kangjiuzui,
                    Kanghunluan = kanghunluan,
                    Kangyiwang = kangyiwang,
                    Bisha = bisha,
                    Fanji = fanji,
                    Minzhong = minzhong,
                    Mogong = mogong,
                    Di = di,
                    Shui = shui,
                    Huo = huo,
                    Feng = feng,
                };

                return info;
            }

            return null;
        }
    }
}
