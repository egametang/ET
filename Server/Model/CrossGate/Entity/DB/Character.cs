using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 玩家全部信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Character : Entity
    {
        //角色
        public Role Role { get; set; }

        //好友
        public List<Friend> Friend { get; set; }

        //装备
        public List<Item> Equip { get; set; }

        //物品
        public List<Item> Item { get; set; }

        //宠物
        public List<Pet> Pet { get; set; }

        //技能
        public List<Skill> Skill { get; set; }

        //称号
        public List<Title> Title { get; set; }

        //Now任务
        public List<int> NowEvent { get; set; }

        //End任务
        public List<int> EndEvent { get; set; }

        //自定义任务
        public List<int> CustomEvent { get; set; }

        //银行物品
        public List<Item> BankItem { get; set; }

        //银行宠物
        public List<Pet> BankPet { get; set; }
    }
}
