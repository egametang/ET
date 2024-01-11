using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 快捷方法/属性
    /// </summary>
    public partial class YIUILoopScroll<TData, TItemRenderer>
    {
        public int           TotalCount             => m_Owner.totalCount; //总数
        public RectTransform Content                => m_Owner.content;
        public RectTransform CacheRect              => m_Owner.u_CacheRect;
        public int           StartLine              => m_Owner.u_StartLine;                             //可见的第一行
        public int           CurrentLines           => m_Owner.u_CurrentLines;                          //滚动中的当前行数
        public int           TotalLines             => m_Owner.u_TotalLines;                            //总数
        public int           EndLine                => Mathf.Min(StartLine + CurrentLines, TotalLines); //可见的最后一行
        public int           ContentConstraintCount => m_Owner.u_ContentConstraintCount;                //限制 行/列 数
        public float         ContentSpacing         => m_Owner.u_ContentSpacing;                        //间隔
        public int           ItemStart              => m_Owner.u_ItemStart;                             //当前显示的第一个的Index                
        public int           ItemEnd                => m_Owner.u_ItemEnd;                               //当前显示的最后一个index 被+1了注意                      

        //在开始时用startItem填充单元格，同时清除现有的单元格
        public void RefillCells(int startItem = 0, bool fillViewRect = false, float contentOffset = 0)
        {
            m_Owner.RefillCells(startItem, fillViewRect, contentOffset);
        }

        //在结束时重新填充endItem中的单元格，同时清除现有的单元格
        public void RefillCellsFromEnd(int endItem = 0, bool alignStart = false)
        {
            m_Owner.RefillCellsFromEnd(endItem, alignStart);
        }

        public void RefreshCells()
        {
            m_Owner.RefreshCells();
        }

        public void ClearCells()
        {
            m_Owner.ClearCells();
        }

        public int GetFirstItem(out float offset)
        {
            return m_Owner.GetFirstItem(out offset);
        }

        public int GetLastItem(out float offset)
        {
            return m_Owner.GetLastItem(out offset);
        }

        private int GetValidIndex(int index)
        {
            return Mathf.Clamp(index, 0, TotalCount - 1);
        }

        public void ScrollToCell(int index, float speed)
        {
            if (TotalCount <= 0) return;
            m_Owner.ScrollToCell(GetValidIndex(index), speed);
        }

        public void ScrollToCellWithinTime(int index, float time)
        {
            if (TotalCount <= 0) return;
            m_Owner.ScrollToCellWithinTime(GetValidIndex(index), time);
        }

        public void StopMovement()
        {
            m_Owner.StopMovement();
        }
    }
}