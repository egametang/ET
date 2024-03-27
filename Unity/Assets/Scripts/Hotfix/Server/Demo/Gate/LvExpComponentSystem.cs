using System.Linq;

namespace ET.Server
{
    [FriendOf(typeof(LvExpComponent))]
    public static partial class LvExpComponentSystem
    {
        public static void Add(this LvExpComponent self, int addExp)
        {
            int newExp = self.Exp + addExp;
            if (newExp >= 100)
            {
                int addLv = newExp / 100;
                self.Lv += addLv;
                self.Exp = newExp % 100;
            }
            else
            {
                self.Exp = newExp;
            }
        }
    }
}