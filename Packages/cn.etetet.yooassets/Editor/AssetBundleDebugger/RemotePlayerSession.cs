using System;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
    internal class RemotePlayerSession
    {
        private readonly List<DebugReport> _reportList = new List<DebugReport>();

        /// <summary>
        /// 用户ID
        /// </summary>
        public int PlayerId { private set; get; }

        /// <summary>
        /// 保存的报告最大数量
        /// </summary>
        public int MaxReportCount { private set; get; }

        public int MinRangeValue
        {
            get
            {
                return 0;
            }
        }
        public int MaxRangeValue
        {
            get
            {
                int index = _reportList.Count - 1;
                if (index < 0)
                    index = 0;
                return index;
            }
        }


        public RemotePlayerSession(int playerId, int maxReportCount = 1000)
        {
            PlayerId = playerId;
            MaxReportCount = maxReportCount;
        }

        /// <summary>
        /// 清理缓存数据
        /// </summary>
        public void ClearDebugReport()
        {
            _reportList.Clear();
        }

        /// <summary>
        /// 添加一个调试报告
        /// </summary>
        public void AddDebugReport(DebugReport report)
        {
            if (report == null)
                Debug.LogWarning("Invalid debug report data !");

            if (_reportList.Count >= MaxReportCount)
                _reportList.RemoveAt(0);
            _reportList.Add(report);
        }

        /// <summary>
        /// 获取调试报告
        /// </summary>
        public DebugReport GetDebugReport(int rangeIndex)
        {
            if (_reportList.Count == 0)
                return null;
            if (rangeIndex < 0 || rangeIndex >= _reportList.Count)
                return null;
            return _reportList[rangeIndex];
        }

        /// <summary>
        /// 规范索引值
        /// </summary>
        public int ClampRangeIndex(int rangeIndex)
        {
            if (rangeIndex < 0)
                return 0;

            if (rangeIndex > MaxRangeValue)
                return MaxRangeValue;

            return rangeIndex;
        }
    }
}