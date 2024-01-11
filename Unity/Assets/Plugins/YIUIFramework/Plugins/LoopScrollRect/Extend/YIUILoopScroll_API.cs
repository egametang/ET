using System.Collections.Generic;
using System.Linq;
using ET.Client;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 对外可调用API
    /// </summary>
    public partial class YIUILoopScroll<TData, TItemRenderer>
    {
        //设置数据 然后刷新
        //不管是要修改数据长度 还是数据变更了 都用此方法刷新
        public void SetDataRefresh(IList<TData> data)
        {
            m_Data             = data;
            m_Owner.totalCount = data.Count;
            RefreshCells();
        }

        //刷新时默认选中某个索引数据
        //注意这里相当于+=操作 如果你会频繁调用这个方法
        //又想每次刷新选中不同的索引
        //那么你应该先自行调用一次 ClearSelect
        public void SetDataRefresh(IList<TData> data, int index)
        {
            SetDefaultSelect(index);
            SetDataRefresh(data);
        }

        //同上 请看注释 注意使用方式
        public void SetDataRefresh(IList<TData> data, List<int> index)
        {
            SetDefaultSelect(index);
            SetDataRefresh(data);
        }

        //如果 < 0 则表示这个对象在对象池里
        public int GetItemIndex(TItemRenderer item)
        {
            return GetItemIndex(item.GetParent<YIUIComponent>().OwnerRectTransform);
        }

        //只能获取当前可见的对象
        public TItemRenderer GetItemByIndex(int index, bool log = true)
        {
            if (index < ItemStart || index >= ItemEnd) return null;
            var childIndex = index - ItemStart;
            if (childIndex < 0 || childIndex >= Content.childCount)
            {
                if (log)
                {
                    Debug.LogError($"索引错误 请检查 index:{index} Start:{ItemStart} childIndex:{childIndex} childCount:{Content.childCount}");
                }

                return null;
            }

            var transform = Content.GetChild(childIndex);
            var uiBase    = GetItemRendererByDic(transform);
            return uiBase;
        }

        //判断某个对象是否被选中
        public bool IsSelect(TItemRenderer item)
        {
            return m_OnClickItemHashSet.Contains(GetItemIndex(item));
        }

        //就获取目前显示的这几个数据
        public List<TData> GetShowData()
        {
            var listData = new List<TData>();

            for (var i = ItemStart; i < ItemEnd; i++)
            {
                listData.Add(m_Data[i]);
            }

            return listData;
        }

        #region 点击相关 获取被选中目标..

        //获取当前所有被选择的索引
        public List<int> GetSelectIndex()
        {
            return m_OnClickItemQueue.ToList();
        }

        //只能得到当前可见的 不可见的拿不到
        public List<TItemRenderer> GetSelectItem()
        {
            var selectList = new List<TItemRenderer>();
            foreach (var index in GetSelectIndex())
            {
                var item = GetItemByIndex(index);
                if (item != null)
                {
                    selectList.Add(item);
                }
            }

            return selectList;
        }

        //获取所有被选择的数据
        public List<TData> GetSelectData()
        {
            var selectList = new List<TData>();
            foreach (var index in GetSelectIndex())
            {
                selectList.Add(m_Data[index]);
            }

            return selectList;
        }

        //移除某个选中的目标 然后刷新
        public void RemoveSelectIndexRefresh(int index)
        {
            RemoveSelectIndex(index);
            RefreshCells();
        }

        #endregion
    }
}