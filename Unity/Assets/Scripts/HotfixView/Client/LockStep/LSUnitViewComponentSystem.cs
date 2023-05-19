using UnityEngine;

namespace ET.Client
{
    public static partial class LSUnitViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitViewComponent self)
        {
            Room room = self.Room();
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                GameObject bundleGameObject = (GameObject)ResourcesComponent.Instance.GetAsset("Unit.unity3d", "Unit");
                GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, GlobalComponent.Instance.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                lsUnitView.AddComponent<LSAnimatorComponent>();
            }
        }
    }
}