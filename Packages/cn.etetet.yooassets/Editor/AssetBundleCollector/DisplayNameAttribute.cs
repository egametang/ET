using System;
using System.Reflection;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器显示名字
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName;

        public DisplayNameAttribute(string name)
        {
            this.DisplayName = name;
        }
    }

    public static class DisplayNameAttributeHelper
    {
        internal static T GetAttribute<T>(Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T), false);
        }

        internal static T GetAttribute<T>(MethodInfo methodInfo) where T : Attribute
        {
            return (T)methodInfo.GetCustomAttribute(typeof(T), false);
        }

        internal static T GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            return (T)field.GetCustomAttribute(typeof(T), false);
        }
    }
}