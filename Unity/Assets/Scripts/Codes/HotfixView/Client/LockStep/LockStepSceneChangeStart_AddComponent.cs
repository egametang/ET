using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LockStepSceneChangeStart_AddComponent: AEvent<Scene, EventType.LockStepSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, EventType.LockStepSceneChangeStart args)
        {
            Room room = clientScene.GetComponent<Room>();
            
            // 创建loading界面
            
            // 删除大厅UI
            await UIHelper.Remove(clientScene, UIType.UILobby);
            
            // 加载场景资源
            await ResourcesComponent.Instance.LoadBundleAsync($"{room.Name}.unity3d");
            // 切换到map场景

            await SceneManager.LoadSceneAsync(room.Name);

            room.AddComponent<LSUnitViewComponent>();
        }
    }
}