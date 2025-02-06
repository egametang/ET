using System.IO;

namespace ET.PackageManager.Editor
{
    public static partial class StrUtil
    {
        public static string Concat(string str1, string str2)
        {
            var sb = SbPool.Get();
            sb.Append(str1).Append(str2);
            return SbPool.PutAndToStr(sb);
        }

        public static string Concat(string str1, string str2, string str3)
        {
            var sb = SbPool.Get();
            sb.Append(str1).Append(str2).Append(str3);
            return SbPool.PutAndToStr(sb);
        }

        public static string Concat(params string[] param)
        {
            var sb = SbPool.Get();
            for (int i = 0; i < param.Length; i++)
            {
                sb.Append(param[i]);
            }

            return SbPool.PutAndToStr(sb);
        }

        public static string Concat(params object[] param)
        {
            var sb = SbPool.Get();
            for (int i = 0; i < param.Length; i++)
            {
                sb.Append(param[i]);
            }

            return SbPool.PutAndToStr(sb);
        }

        /// <summary>
        /// 按头字母大写来格式化名字
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            StringWriter sw      = new StringWriter();
            bool         isFirst = true;
            for (int index = 0; index < name.Length; index++)
            {
                char c = name[index];
                if (c == '_')
                {
                    isFirst = true;
                    continue;
                }

                if (isFirst)
                {
                    if (c >= 'a' && c <= 'z')
                    {
                        c = char.ToUpper(c);
                    }

                    isFirst = false;
                }

                sw.Write(c);
            }

            sw.Close();
            return sw.ToString();
        }

        /// <summary>
        /// 替换路径中的扩展名
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newExtension"></param>
        /// <returns></returns>
        public static string ReplacePathExtension(string path, string newExtension)
        {
            var extIndex = path.LastIndexOf('.');
            if (extIndex > -1)
            {
                path = path.Substring(0, extIndex);
            }

            if (newExtension[0] == '.')
            {
                return path + newExtension;
            }

            return path + "." + newExtension;
        }

        /// <summary>
        /// 替换文本中的占位符
        /// </summary>
        /// <param name="template"></param>
        /// <param name="keyName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ReplaceKeyToValue(string template, string keyName, params string[] values)
        {
            var sb     = SbPool.Get();
            var chunks = GetStrChunk(template);
            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                if (chunk.KeyIndex == 0)
                {
                    sb.Append(chunk.TextOrKey);
                    continue;
                }

                if (chunk.TextOrKey != keyName ||
                    chunk.KeyIndex > values.Length)
                {
                    sb.Append('{')
                      .Append(chunk.TextOrKey)
                      .Append(chunk.KeyIndex)
                      .Append('}');
                    continue;
                }

                sb.Append(values[chunk.KeyIndex - 1]);
            }

            return SbPool.PutAndToStr(sb);
        }

        /// <summary>
        /// 替换字符串
        /// 这个方法用于替换翔进的ReplaceString
        /// 注意，因为用到非加锁缓存，所以不得用于多线程
        /// </summary>
        /// <param name="text"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ReplaceText(string text, params string[] param)
        {
            return ReplaceKeyToValue(text, "text", param);
        }

        /// <summary>
        /// 将字符串里的英文大写转小写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToLower(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }

            var  length    = s.Length;
            bool hasChange = false;
            var  sb        = SbPool.Get();
            for (int i = 0; i < length; i++)
            {
                char c = s[i];
                if (c >= 'A' && c <= 'Z')
                {
                    hasChange = true;
                    sb.Append(char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (hasChange)
            {
                return SbPool.PutAndToStr(sb);
            }

            //如果没有改变，省一个toString
            SbPool.Put(sb);
            return s;
        }
    }
}
