using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [Event(EventIdType.LoginEnterMap)]
    public class Event_LoginEnterMap: AEvent
    {
        public override async void Run()
        {
            long userid = ClientComponent.Instance.LocalUser.UserID;
            LoginBasicRoleInfo info = ClientComponent.Instance.LocalRole.BasicInfo;

            //等待10毫秒
            await ETModel.Game.Scene.GetComponent<TimerComponent>().WaitAsync(10);

            //加载生成地图
            MapComponent.Instance.Init(info.RoleInfo.Dong, info.RoleInfo.Nan, info.RoleInfo.MapID);
            
            //持有本地玩家信息
            GamePlayer gamePlayer = GamePlayerFactory.Create(userid, info.RoleInfo.Level, info.RoleInfo.CharacterID, info.RoleInfo.Name, "todo", info.RoleInfo.Dong, info.RoleInfo.Nan);
            GamePlayerComponent.Instance.MyPlayer = gamePlayer;

            //加载场景玩家信息
            SceneObject sceneObject = SceneObjectFactory.CreateScenePlayer(gamePlayer.UserID, gamePlayer.CharacterID, gamePlayer.Level, gamePlayer.PlayerName, gamePlayer.Title);
            sceneObject.MyGameobject.transform.localPosition = MapHelper.ConverCordToVector3(info.RoleInfo.Dong, info.RoleInfo.Nan);
            SceneObjectComponent.Instance.Add(sceneObject);

            //设置摄像机位置
            Camera.main.transform.SetParent(sceneObject.MyGameobject.transform, false);
            Camera.main.transform.localPosition = new Vector3(0f, 0f, -1f);

            //关闭加载界面
            Game.Scene.GetComponent<UIComponent>().Close(UIType.UILoadingScene);
        }
    }
}