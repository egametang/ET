using ET.EventType;
using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.LockStepClient)]
    public class LockStepAfterUnitCreate_CreateUnitFView: AEvent<LSWorld, LSAfterUnitCreate>
    {
        protected override async ETTask Run(LSWorld lsWorld, LSAfterUnitCreate args)
        {
            Room room = lsWorld.GetParent<Room>();
            LSUnitViewComponent lsUnitViewComponent = room.GetComponent<LSUnitViewComponent>();

            if (lsUnitViewComponent == null)
            {
                return;
            }
            
            GameObject bundleGameObject = (GameObject)ResourcesComponent.Instance.GetAsset("Unit.unity3d", "Unit");
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
            
            GameObject go = UnityEngine.Object.Instantiate(prefab, GlobalComponent.Instance.Unit, true);
            go.transform.position = args.LsUnit.Position.ToVector();

            LSUnitView lsUnitView = lsUnitViewComponent.AddChildWithId<LSUnitView, GameObject>(args.LsUnit.Id, go);
            lsUnitView.AddComponent<LSAnimatorComponent>();

            await ETTask.CompletedTask;
        }
    }
}