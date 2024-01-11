#if UNITY_EDITOR

using System.Collections.Generic;
using System.Text;

namespace YIUIFramework.Editor
{
    internal static class UICreateRedDotKeyGet
    {
        public static string Get(List<RedDotKeyData> dataList)
        {
            var sb = SbPool.Get();
            foreach (var data in dataList)
            {
                data.AddKeyData(sb);
            }

            return SbPool.PutAndToStr(sb);
        }

        private const string m_EnumContent = @"
        [LabelText(""{0}"")]
        Key{1} = {1},
";

        private static void AddKeyData(this RedDotKeyData data, StringBuilder sb)
        {
            var des = string.IsNullOrEmpty(data.Des) ? data.Id.ToString() : data.Des;
            sb.AppendFormat(m_EnumContent, des, data.Id);
        }
    }
}
#endif