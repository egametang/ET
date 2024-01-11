#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace YIUIFramework.Editor
{
    /// <summary>
    /// 界面的枚举
    /// </summary>
    public static class UICreatePanelViewEnum
    {
        public static string Get(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetEventTable(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GetEventTable(this UIBindCDETable self, StringBuilder sb)
        {
            var splitData = self.PanelSplitData;
            if (splitData == null) return;
            if (!splitData.ShowCreatePanelViewEnum()) return;
            if (!splitData.CreatePanelViewEnum) return;
            var index = 1;

            sb.Append("    /// <summary>\n    /// 由YIUI工具自动创建 请勿修改\n    /// 当前Panel所有可用view枚举\n    /// </summary>\n");
            sb.AppendFormat("    public enum E{0}ViewEnum\r\n    {{\r\n", self.name);
            AddViewEnum(splitData.AllCommonView, sb, ref index);
            AddViewEnum(splitData.AllCreateView, sb, ref index);
            AddViewEnum(splitData.AllPopupView, sb, ref index);
            sb.Append("    }");
        }

        private static void AddViewEnum(List<RectTransform> viewList, StringBuilder sb, ref int index)
        {
            foreach (var viewParent in viewList)
            {
                var viewName = viewParent.name.Replace(UIStaticHelper.UIParentName, "");
                sb.AppendFormat("        {0} = {1},\r\n", $"{viewName}", index);
                index++;
            }
        }
    }
}
#endif