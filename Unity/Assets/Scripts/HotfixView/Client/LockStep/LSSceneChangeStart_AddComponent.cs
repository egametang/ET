using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LSSceneChangeStart_AddComponent: AEvent<Scene, EventType.LSSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, EventType.LSSceneChangeStart args)
        {
            Room room = clientScene.GetComponent<Room>();
            room.AddComponent<ResourcesLoaderComponent>();
            room.AddComponent<UIComponent>();
            
            // 创建loading界面
            
            
            // 创建房间UI
            await UIHelper.Create(args.Room, UIType.UILSRoom, UILayer.Low);
            
            // 加载场景资源
            ResourcesComponent resourcesComponent = clientScene.Root().GetComponent<ResourcesComponent>();
            await resourcesComponent.LoadBundleAsync($"{room.Name}.unity3d");
            // 切换到map场景

            await SceneManager.LoadSceneAsync(room.Name);
        }
    }
}