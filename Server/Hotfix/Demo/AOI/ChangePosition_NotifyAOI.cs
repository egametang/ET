using UnityEngine;

namespace ET
{
    [Event]
    public class ChangePosition_NotifyAOI: AEvent<EventType.ChangePosition>
    {
        protected override async ETTask Run(EventType.ChangePosition args)
        {
            await ETTask.CompletedTask;
            Vector3 oldPos = args.OldPos;
            Unit unit = args.Unit;
            int oldCellX = (int) (oldPos.x * 1000) / AOIManagerComponent.CellSize;
            int oldCellY = (int) (oldPos.z * 1000) / AOIManagerComponent.CellSize;
            int newCellX = (int) (unit.Position.x * 1000) / AOIManagerComponent.CellSize;
            int newCellY = (int) (unit.Position.z * 1000) / AOIManagerComponent.CellSize;
            if (oldCellX == newCellX && oldCellY == newCellY)
            {
                return;
            }

            AOIEntity aoiEntity = unit.GetComponent<AOIEntity>();
            if (aoiEntity == null)
            {
                return;
            }

            unit.Domain.GetComponent<AOIManagerComponent>().Move(aoiEntity, newCellX, newCellY);
        }
    }
}