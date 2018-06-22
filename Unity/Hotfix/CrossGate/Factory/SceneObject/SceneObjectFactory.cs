using UnityEngine;

namespace ETHotfix
{
    public static class SceneObjectFactory
    {
        private static readonly Vector3 NamePos = new Vector3(0f, 0.8f, 0f);
        private static readonly Vector3 TitlePos = new Vector3(0f, 0.95f, 0f);
        private static readonly Vector3 BattlePos = new Vector3(0f, 0.05f, 0f);

        public static SceneObject CreateScenePlayer(long userid, int characterid, int level, string playername, string title)
        {
            //创建游戏实体
            SceneObject player = ComponentFactory.CreateWithId<SceneObject>(userid);
            player.UserID = userid;
            player.MyGameobject = GameObjectPoolComponent.Instance.Get(GameObjectType.SceneObject);
            player.Init(level, playername, title);

            //设置父节点
            GameObject parent = GameObject.Find($"/Global/Unit");
            player.MyGameobject.transform.SetParent(parent.transform, false);

            //加入动画组件
            //SpriteAnimatorComponent sprite = player.AddComponent<SpriteAnimatorComponent>();
            //sprite.Init(characterid);
            
            //加入文字组件
            SceneTextComponent text = player.AddComponent<SceneTextComponent>();
            GameObject nametext = GameObjectPoolComponent.Instance.Get(GameObjectType.SceneText);
            nametext.transform.SetParent(player.MyGameobject.transform);
            nametext.transform.localPosition = NamePos;
            text.SetName(nametext, (userid == ClientComponent.Instance.LocalUser.UserID? Color.cyan : Color.white), playername, level);
            if (!string.IsNullOrEmpty(title))
            {
                GameObject titletext = GameObjectPoolComponent.Instance.Get(GameObjectType.SceneText);
                titletext.transform.SetParent(player.MyGameobject.transform);
                titletext.transform.localPosition = TitlePos;
                text.SetTitle(titletext, Color.green, title); //todo 家族称号颜色区分
            }

            //加入移动组件
            player.AddComponent<MovementComponent>();

            return player;
        }
    }
}