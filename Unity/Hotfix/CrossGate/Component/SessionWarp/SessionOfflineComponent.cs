namespace ETHotfix
{
    /// <summary>
    /// 用于Session断开时触发下线
    /// </summary>
    public class SessionOfflineComponent : Component
    {
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            //移除连接组件
            Game.Scene.RemoveComponent<SessionComponent>();
            Game.Scene.ModelScene.RemoveComponent<ETModel.SessionComponent>();

            //释放本地数据
            ClientComponent clientComponent = Game.Scene.GetComponent<ClientComponent>();
            if (clientComponent.LocalRole != null)
            {
                clientComponent.LocalRole.Dispose();
                clientComponent.LocalRole = null;
            }
            if (clientComponent.LocalUser != null)
            {
                clientComponent.LocalUser.Dispose();
                clientComponent.LocalUser = null;
            }

            //持有游戏总UI
            UIComponent uiComponent = Game.Scene.GetComponent<UIComponent>();

            //游戏已经关闭，不用回到登录界面
            if (uiComponent == null || uiComponent.IsDisposed) return;

            //显示掉线界面
            uiComponent.Create(UIType.UIDisconnect);
        }
    }
}
