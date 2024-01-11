using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="UnityEngine.Quaternion"/>.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// 从字符串中解析四元素
        /// </summary>
        public static Quaternion Parse(string text)
        {
            var   elements = text.Substring(1, text.Length - 2).Split(',');
            float x        = float.Parse(elements[0]);
            float y        = float.Parse(elements[1]);
            float z        = float.Parse(elements[2]);
            float w        = float.Parse(elements[3]);
            return new Quaternion(x, y, z, w);
        }
    }
}