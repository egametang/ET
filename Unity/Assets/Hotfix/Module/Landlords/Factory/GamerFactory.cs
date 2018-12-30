using ETModel;

namespace ETHotfix
{
    public static class GamerFactory
    {
        /// <summary>
        /// 创建玩家对象
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isReady"></param>
        /// <returns></returns>
        public static Gamer Create(long userId, bool isReady)
        {
            Gamer gamer = ComponentFactory.Create<Gamer>();
            gamer.UserID = userId;
            gamer.IsReady = isReady;

            //todo 每个角色分配的ui
            //gamer.AddComponent<GamerUIComponent>();

            return gamer;
        }
    }
}
