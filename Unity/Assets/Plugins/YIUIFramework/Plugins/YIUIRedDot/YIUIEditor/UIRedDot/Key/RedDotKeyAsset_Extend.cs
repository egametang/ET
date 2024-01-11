#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YIUIFramework
{
    public static class RedDotKeyAsset_Extend
    {
        internal static List<RedDotKeyData> GetDataList(this RedDotKeyAsset self)
        {
            var listData = new List<RedDotKeyData>();

            foreach (var data in self.AllRedDotDic.Values)
            {
                listData.Add(data);
            }

            listData.Sort(OnSortData);

            return listData;
        }

        private static int OnSortData(RedDotKeyData x, RedDotKeyData y)
        {
            return x.Id < y.Id ? -1 : 1;
        }

        //获取当前空闲的ID 安全的ID 直接用这个ID
        //建议就从ID 1开始 我不信你有超过MAX的需求
        internal static int GetSafetyId(this RedDotKeyAsset self)
        {
            if (self.AllRedDotDic.Count <= 0)
            {
                return 1;
            }

            var listData = GetDataList(self);
            var id       = 1;
            for (var i = 0; i < listData.Count; i++)
            {
                var data   = listData[i];
                var dataId = data.Id;
                if (id != dataId)
                {
                    if (!self.AllRedDotDic.ContainsKey(id))
                    {
                        break;
                    }
                }

                id = dataId + 1;
            }

            if (id >= int.MaxValue)
            {
                Debug.LogError($"ID 不能>= {int.MaxValue} ID不可能设计这么大 请检查");
                return 1;
            }

            return id;
        }

        internal static bool ContainsKey(this RedDotKeyAsset self, int id)
        {
            return self.AllRedDotDic.ContainsKey(id);
        }

        internal static (bool, string) AddKey(this RedDotKeyAsset self, RedDotKeyData data)
        {
            if (data.Id <= 0)
            {
                const string tips = "ID <= 0 是不允许的";
                Debug.LogError(tips);
                return (false, tips);
            }

            if (self.AllRedDotDic.ContainsKey(data.Id))
            {
                var tips = $"当前ID已经存在 无法重复添加 {data.Id}";
                Debug.LogError(tips);
                return (false, tips);
            }

            self.Add(data);
            return (true, $"成功添加ID {data.Id}  {data.Des}");
        }

        internal static (bool, string) RemoveKey(this RedDotKeyAsset self, int id)
        {
            if (!self.AllRedDotDic.ContainsKey(id))
            {
                var tips = $"当前ID不存在 无法移除 {id}";
                Debug.LogError(tips);
                return (false, tips);
            }

            //移除前 需执行判断关联信息 提示删除警告
            self.Remove(id);
            return (true, $"成功移除ID {id}");
        }

        internal static (bool, string) ChangeKey(this RedDotKeyAsset self, int lastId, RedDotKeyData changeData)
        {
            if (!self.AllRedDotDic.ContainsKey(lastId))
            {
                var tips = $"修改的Id 不存在 {lastId}";
                Debug.LogError(tips);
                return (false, tips);
            }

            var lastData = self.AllRedDotDic[lastId];

            if (lastId == changeData.Id)
            {
                if (lastData.Des == changeData.Des) return (false, $"修改前后数据相同 无法修改 请检查");
                lastData.ChangeDes(changeData.Des);
                return (true, $"成功修改 {lastId} 描述 >> {changeData.Des}");
            }

            if (self.AllRedDotDic.ContainsKey(changeData.Id))
            {
                var tips = $"修改后的Id {changeData.Id} 已存在 无法修改 {lastId}";
                Debug.LogError(tips);
                return (false, tips);
            }

            self.Remove(lastData);
            self.Add(new RedDotKeyData(changeData.Id, changeData.Des));

            //修改前 需执行判断关联信息 提示警告
            return (true, $"修改完成 {lastId} >> {changeData.Id}  >> {changeData.Des}");
        }

        internal static (RedDotKeyData, string) GetKey(this RedDotKeyAsset self, int id)
        {
            if (!self.AllRedDotDic.ContainsKey(id))
            {
                var tips = $"当前ID不存在 {id}";
                Debug.LogError(tips);
                return (null, tips);
            }

            var data = self.AllRedDotDic[id];
            return (data, $"查询到数据 ID:{id} Des:{data.Des}");
        }

        private static void Add(this RedDotKeyAsset self, RedDotKeyData data)
        {
            if (self.ContainsKey(data.Id))
            {
                Debug.LogError($"当前ID 已存在无法重复添加 请检查");
                return;
            }

            self.m_AllRedDotDic.Add(data.Id, data);
            EditorUtility.SetDirty(self);
        }

        private static void Remove(this RedDotKeyAsset self, RedDotKeyData data)
        {
            Remove(self, data.Id);
        }

        private static void Remove(this RedDotKeyAsset self, int id)
        {
            self.m_AllRedDotDic.Remove(id);
            EditorUtility.SetDirty(self);
        }
    }
}

#endif