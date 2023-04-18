using ET.EventType;
using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.LockStepClient)]
    public class LockStepAfterUnitCreate_CreateUnitFView: AEvent<LSWorld, LSAfterUnitCreate>
    {
        protected override async ETTask Run(LSWorld lsWorld, LSAfterUnitCreate args)
        {
            BattleScene battleScene = lsWorld.GetParent<BattleScene>();
            UnitFViewComponent unitFViewComponent = battleScene.GetComponent<UnitFViewComponent>();

            if (unitFViewComponent == null)
            {
                return;
            }
            
            GameObject bundleGameObject = (GameObject)ResourcesComponent.Instance.GetAsset("Unit.unity3d", "Unit");
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
            
            GameObject go = UnityEngine.Object.Instantiate(prefab, GlobalComponent.Instance.Unit, true);
            go.transform.position = args.LsUnit.Position.ToVector();

            unitFViewComponent.AddChild<UnitFView, GameObject>(go);

            await ETTask.CompletedTask;
        }
    }
}