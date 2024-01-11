using System.Text.RegularExpressions;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public static class NameUtility
    {
        //统一命名
        public const string FirstName     = "u_"; //不使用m_ 这个是给具体逻辑用的 u_ 就知道这个是自动创建的 也避免命名冲突
        public const string ComponentName = "Com";
        public const string DataName      = "Data";
        public const string EventName     = "Event";
        public const string UIName        = "UI";

        //名字只允许以下的正则表达式
        public const string NameRegex = "[^a-z0-9A-Z_]";

        //m_必须大写
        public const bool Big = true;

        public static bool CheckFirstName(this string name, string otherName)
        {
            return !string.IsNullOrEmpty(name) && name.StartsWith($"{FirstName}{otherName}");
        }

        public static bool CheckComponentName(this string name)
        {
            return !string.IsNullOrEmpty(name) && name.StartsWith($"{FirstName}{ComponentName}");
        }

        public static bool CheckDataName(this string name)
        {
            return !string.IsNullOrEmpty(name) && name.StartsWith($"{FirstName}{DataName}");
        }

        public static bool CheckEventName(this string name)
        {
            return !string.IsNullOrEmpty(name) && name.StartsWith($"{FirstName}{EventName}");
        }

        public static bool CheckUIName(this string name)
        {
            return !string.IsNullOrEmpty(name) && name.StartsWith($"{FirstName}{UIName}");
        }

        /// <summary>
        /// 必须满足m_
        /// 然后吧目标第3个字母改为大写
        /// 返回改变后的字符串 如果相同就没有修改
        /// 如果不同就是被修改了
        /// </summary>
        public static string ChangeToBigName(this string name, string otherName)
        {
            if (!Big)
            {
                return name;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.LogError($"名字是空的 无法判断 请检查");
                return name;
            }

            if (!CheckFirstName(name, otherName))
            {
                Logger.LogError($"当前命名 不符合 {FirstName}{otherName} 规范 无法检查是否大写");
                return name;
            }

            var minLength = FirstName.Length + otherName.Length;

            return ToUpperByIndex(name, (uint)minLength);
        }

        public static string ToFirstUpper(string name)
        {
            return ToUpperByIndex(name, 0);
        }

        public static string GetQualifiedName(string name)
        {
            if (name.IndexOf("(Clone)") > -1)
            {
                name = name.Replace("(Clone)", string.Empty);
            }

            name = Regex.Replace(name, NameRegex, ""); //替换所有非法字符

            return name;
        }

        public static string ToUpperByIndex(string name, uint index)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.LogError($"名字是空的 无法判断 请检查");
                return name;
            }

            name = GetQualifiedName(name);

            var arrayName = name.ToCharArray();
            if (arrayName.Length <= index)
            {
                Logger.LogError($"替换后名称长度不符 {name} 长度:{arrayName.Length} < {index}");
                return name;
            }

            var charUpper = arrayName[index];
            if (charUpper >= 'A' && charUpper <= 'Z')
            {
                return name;
            }

            if (charUpper >= 'a' && charUpper <= 'z')
            {
                var charTo = charUpper.ToString().ToUpper().ToCharArray();
                arrayName[index] = charTo[0];
            }
            else
            {
                arrayName[index] = 'A';
            }

            return new string(arrayName);
        }
    }
}