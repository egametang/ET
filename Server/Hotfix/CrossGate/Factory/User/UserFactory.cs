using ETModel;

namespace ETHotfix
{
    public static class UserFactory
    {
        /// <summary>
        /// 创建User对象
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static User Create(long userId, long sessionId)
        {
            User user = ComponentFactory.Create<User, long>(userId);
            user.AddComponent<UnitGateComponent, long>(sessionId);
            Game.Scene.GetComponent<UserComponent>().Add(user);
            return user;
        }
    }
}
