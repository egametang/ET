using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(AOIManagerComponent))]
    [FriendOf(typeof(AOIEntity))]
    [FriendOf(typeof(Cell))]
    public static partial class AOIManagerComponentSystem
    {
        public static void Add(this AOIManagerComponent self, AOIEntity aoiEntity, float x, float y)
        {
            int cellX = (int)(x * 1000) / AOIManagerComponent.CellSize;
            int cellY = (int)(y * 1000) / AOIManagerComponent.CellSize;

            if (aoiEntity.ViewDistance == 0)
            {
                aoiEntity.ViewDistance = 1;
            }

            AOIHelper.CalcEnterAndLeaveCell(aoiEntity, cellX, cellY, aoiEntity.SubEnterCells, aoiEntity.SubLeaveCells);

            // 遍历EnterCell
            foreach (long cellId in aoiEntity.SubEnterCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.SubEnter(cell);
            }

            // 遍历LeaveCell
            foreach (long cellId in aoiEntity.SubLeaveCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.SubLeave(cell);
            }

            // 自己加入的Cell
            Cell selfCell = self.GetCell(AOIHelper.CreateCellId(cellX, cellY));
            aoiEntity.Cell = selfCell;
            selfCell.Add(aoiEntity);
            // 通知订阅该Cell Enter的Unit
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in selfCell.SubsEnterEntities)
            {
                AOIEntity e = kv.Value;
                e.EnterSight(aoiEntity);
            }
        }

        public static void Remove(this AOIManagerComponent self, AOIEntity aoiEntity)
        {
            if (aoiEntity.Cell == null)
            {
                return;
            }

            // 通知订阅该Cell Leave的Unit
            aoiEntity.Cell.Remove(aoiEntity);
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in aoiEntity.Cell.SubsLeaveEntities)
            {
                AOIEntity e = kv.Value;
                e?.LeaveSight(aoiEntity);
            }

            // 通知自己订阅的Enter Cell，清理自己
            foreach (long cellId in aoiEntity.SubEnterCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubEnter(cell);
            }

            foreach (long cellId in aoiEntity.SubLeaveCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubLeave(cell);
            }
    
            // 检查
            if (aoiEntity.SeeUnits.Count > 1)
            {
                Log.Error($"aoiEntity has see units: {aoiEntity.SeeUnits.Count}");
            }

            if (aoiEntity.BeSeeUnits.Count > 1)
            {
                Log.Error($"aoiEntity has beSee units: {aoiEntity.BeSeeUnits.Count}");
            }
        }

        private static Cell GetCell(this AOIManagerComponent self, long cellId)
        {
            Cell cell = self.GetChild<Cell>(cellId);
            if (cell == null)
            {
                cell = self.AddChildWithId<Cell>(cellId);
            }

            return cell;
        }

        public static void Move(AOIEntity aoiEntity, Cell newCell, Cell preCell)
        {
            aoiEntity.Cell = newCell;
            preCell.Remove(aoiEntity);
            newCell.Add(aoiEntity);
            // 通知订阅该newCell Enter的Unit
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in newCell.SubsEnterEntities)
            {
                AOIEntity e = kv.Value;
                if (e.SubEnterCells.Contains(preCell.Id))
                {
                    continue;
                }
                e.EnterSight(aoiEntity);
            }

            // 通知订阅preCell leave的Unit
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in preCell.SubsLeaveEntities)
            {
                // 如果新的cell仍然在对方订阅的subleave中
                AOIEntity e = kv.Value;
                if (e.SubLeaveCells.Contains(newCell.Id))
                {
                    continue;
                }

                e.LeaveSight(aoiEntity);
            }
        }

        public static void Move(this AOIManagerComponent self, AOIEntity aoiEntity, int cellX, int cellY)
        {
            long newCellId = AOIHelper.CreateCellId(cellX, cellY);
            if (aoiEntity.Cell.Id == newCellId) // cell没有变化
            {
                return;
            }

            // 自己加入新的Cell
            Cell newCell = self.GetCell(newCellId);
            Move(aoiEntity, newCell, aoiEntity.Cell);

            AOIHelper.CalcEnterAndLeaveCell(aoiEntity, cellX, cellY, aoiEntity.enterHashSet, aoiEntity.leaveHashSet);

            // 算出自己leave新Cell
            foreach (long cellId in aoiEntity.leaveHashSet)
            {
                if (aoiEntity.SubLeaveCells.Contains(cellId))
                {
                    continue;
                }

                Cell cell = self.GetCell(cellId);
                aoiEntity.SubLeave(cell);
            }

            // 算出需要通知离开的Cell
            aoiEntity.SubLeaveCells.ExceptWith(aoiEntity.leaveHashSet);
            foreach (long cellId in aoiEntity.SubLeaveCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubLeave(cell);
            }

            // 这里交换两个HashSet,提高性能
            ObjectHelper.Swap(ref aoiEntity.SubLeaveCells, ref aoiEntity.leaveHashSet);

            // 算出自己看到的新Cell
            foreach (long cellId in aoiEntity.enterHashSet)
            {
                if (aoiEntity.SubEnterCells.Contains(cellId))
                {
                    continue;
                }

                Cell cell = self.GetCell(cellId);
                aoiEntity.SubEnter(cell);
            }

            // 离开的Enter
            aoiEntity.SubEnterCells.ExceptWith(aoiEntity.enterHashSet);
            foreach (long cellId in aoiEntity.SubEnterCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubEnter(cell);
            }

            // 这里交换两个HashSet,提高性能
            ObjectHelper.Swap(ref aoiEntity.SubEnterCells, ref aoiEntity.enterHashSet);
        }
    }
}