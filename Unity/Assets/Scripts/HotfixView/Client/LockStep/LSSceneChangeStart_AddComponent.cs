using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LSSceneChangeStart_AddComponent: AEvent<Scene, LSSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, LSSceneChangeStart args)
        {
            Room room = clientScene.GetComponent<Room>();
            room.AddComponent<ResourcesLoaderComponent>();
            room.AddComponent<UIComponent>();
            
            // 创建loading界面
            
            
            // 创建房间UI
            await UIHelper.Create(args.Room, UIType.UILSRoom, UILayer.Low);
            
            // 加载场景资源
            SceneLoaderComponent sceneLoaderComponent = room.AddComponent<SceneLoaderComponent>();
            await sceneLoaderComponent.LoadSceneAsync($"Assets/Scenes/{room.Name}.unity");
        }
    }
}