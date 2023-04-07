using ET.EventType;
using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class LockStepAfterUnitCreate_CreateUnitFView: AEvent<LockStepAfterUnitCreate>
    {
        protected override async ETTask Run(Scene currentScene, LockStepAfterUnitCreate args)
        {
            UnitFViewComponent unitFViewComponent = currentScene.GetComponent<UnitFViewComponent>();
            
            GameObject bundleGameObject = (GameObject)ResourcesComponent.Instance.GetAsset("Unit.unity3d", "Unit");
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
            
            GameObject go = UnityEngine.Object.Instantiate(prefab, GlobalComponent.Instance.Unit, true);
            go.transform.position = args.UnitF.Position.ToVector();

            unitFViewComponent.AddChild<UnitFView, GameObject>(go);

            await ETTask.CompletedTask;
        }
    }
}