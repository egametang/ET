using UnityEngine;

namespace ET.Server
{
    [Event]
    public class ChangePosition_NotifyAOI: AEvent<ET.EventType.ChangePosition>
    {
        protected override async ETTask Run(ET.EventType.ChangePosition args)
        {
            Unit unit = args.Unit;

            // 机器人也有Unit，机器人的Unit在Current Scene
            
            if (unit.DomainScene().SceneType != SceneType.Map)
            {
                return;
            }
            
            Vector3 oldPos = args.OldPos;
            
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
            await ETTask.CompletedTask;
        }
    }
}