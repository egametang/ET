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
                ResourcesComponent resourcesComponent = self.Fiber().GetComponent<ResourcesComponent>();
                GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset("Unit.unity3d", "Unit");
                GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

                GlobalComponent globalComponent = self.Fiber().GetComponent<GlobalComponent>();
                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                lsUnitView.AddComponent<LSAnimatorComponent>();
            }
        }
    }
}