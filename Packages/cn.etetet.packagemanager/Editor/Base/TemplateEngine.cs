using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// 简单的模板引擎, 主要用在编辑器上
    /// </summary>
    public class TemplateEngine
    {
        private static readonly Dictionary<string, string> g_templateCacheMap = new Dictionary<string, string>();

        /// <summary>
        /// 模板文件的基础路径
        /// eg: Assets/Ediotr/Config/Decoder/Template/
        /// 注意： 前面不要“/”，后面要“/”
        /// </summary>
        public static string TemplateBasePath;

        private const string m_StartSignFormat = "#region {0}开始";
        private const string m_EndSignFormat   = "#endregion {0}结束";

        /// <summary>
        /// 处理模板
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="valueDic"></param>
        /// <param name="valueDic2"></param>
        /// <param name="readTemplateCache"></param>
        /// <returns></returns>
        public static string Do(string                     templatePath, Dictionary<string, string> valueDic,
                                Dictionary<string, string> valueDic2,    bool                       readTemplateCache = true)
        {
            if (!string.IsNullOrEmpty(TemplateBasePath))
            {
                if (TemplateBasePath.Contains("Assets/") && templatePath.Contains("Assets/"))
                {
                    templatePath = templatePath.Replace("Assets/", "");
                }

                templatePath = TemplateBasePath + templatePath;
            }

            string templateStr = null;
            if (readTemplateCache)
            {
                g_templateCacheMap.TryGetValue(templatePath, out templateStr);
            }

            if (templateStr == null)
            {
                string path = EditorHelper.GetProjPath(templatePath);
                try
                {
                    templateStr = File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    Debug.LogError("读取文件失败: " + e);
                    return null;
                }

                g_templateCacheMap[templatePath] = templateStr;
            }

            if (valueDic != null)
            {
                foreach (KeyValuePair<string, string> pair in valueDic)
                {
                    templateStr = templateStr.Replace("${" + pair.Key + "}", pair.Value);
                }
            }

            if (valueDic2 != null)
            {
                foreach (KeyValuePair<string, string> pair in valueDic2)
                {
                    templateStr = templateStr.Replace("${" + pair.Key + "}", pair.Value);
                }
            }

            return templateStr;
        }

        /// <summary>
        ///  处理列表类型模板
        /// </summary>
        /// <typeparam name="TItemType"></typeparam>
        /// <param name="itemTemplateStr"></param>
        /// <param name="list"></param>
        /// <param name="fieldOrPropNames"></param>
        /// <param name="afterProcessItem">后处理函数</param>
        /// <returns></returns>
        public static string DoList<TItemType>(string                          itemTemplateStr, TItemType[] list, string[] fieldOrPropNames,
                                               Func<string, TItemType, string> afterProcessItem = null)
        {
            Type itemType = typeof(TItemType);
            int  len      = list.Length;

            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                TItemType item     = list[i];
                string    template = itemTemplateStr;
                foreach (string name in fieldOrPropNames)
                {
                    string    tName = "${" + name + "}";
                    FieldInfo fi    = itemType.GetField(name);
                    if (fi != null)
                    {
                        template = template.Replace(tName, fi.GetValue(item).ToString());
                        continue;
                    }

                    PropertyInfo pi = itemType.GetProperty(name);
                    if (pi == null)
                    {
                        Debug.LogWarning($"no {name} fields or attributes in the {itemType}");
                        continue;
                    }

                    template = template.Replace(tName, pi.GetValue(item, null).ToString());
                }

                if (afterProcessItem != null)
                {
                    template = afterProcessItem(template, item);
                }

                sb.Append(template);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 检查目标文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool FileExists(string path)
        {
            try
            {
                path = EditorHelper.GetProjPath(path);
                return File.Exists(path);
            }
            catch (Exception e)
            {
                Debug.LogError("检查目标文件是否存在失败: path =" + path + ", err=" + e);
                return false;
            }
        }

        public static bool CreateCodeFile(string path, string templateName, Dictionary<string, string> valueDic)
        {
            templateName = templateName.Contains(".txt") ? templateName : templateName + ".txt";
            string clsStr = Do(templateName, valueDic, null, false);
            if (clsStr == null)
            {
                Debug.LogError("模板转化失败, templateName:" + templateName);
                return false;
            }

            try
            {
                path = EditorHelper.GetProjPath(path);
                string dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Debug.LogError("dir == null");
                    return false;
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, clsStr, Encoding.UTF8);
                Debug.Log(path + "创建成功");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("创建代码文件失败: path =" + path + ", err=" + e);
                return false;
            }
        }

        /// <summary>
        /// 使用Unity #region 为标识的替换规则
        /// 在#region  >> #endregion 这块区域内查找规则替换
        /// </summary>
        /// <param name="otherRetain">标记中的其他的是否保留</param>
        /// <returns></returns>
        private static bool RegionReplace(string path, KeyValuePair<string, string> pair, bool otherRetain = true)
        {
            //获取文件 
            if (path != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                StreamReader  streamReader  = new StreamReader(path);
                string        line          = streamReader.ReadLine();
                bool          isWrite       = false; //标记中途是否检查到已经写入过这个字段 防止重复
                string        startStr      = string.Format(m_StartSignFormat, pair.Key);
                string        endStr        = string.Format(m_EndSignFormat, pair.Key);

                while (line != null)
                {
                    if (line.IndexOf(startStr) > -1)
                    {
                        isWrite = true;
                        stringBuilder.Append(line + "\n");
                    }
                    else if (line.IndexOf(endStr) > -1)
                    {
                        if (isWrite)
                        {
                            stringBuilder.Append($"{pair.Value}\n");
                            isWrite = false;
                        }

                        stringBuilder.Append(line + "\n");
                    }
                    else
                    {
                        if (isWrite)
                        {
                            if (line.IndexOf(pair.Value) > -1)
                            {
                                isWrite = false;
                            }

                            if (otherRetain)
                            {
                                stringBuilder.Append(line + "\n");
                            }
                        }
                        else
                        {
                            stringBuilder.Append(line + "\n");
                        }
                    }

                    line = streamReader.ReadLine();
                }

                streamReader.Close();
                StreamWriter streamWriter = new StreamWriter(path);
                streamWriter.Write(stringBuilder.ToString());
                streamWriter.Close();
            }
            else
            {
                Debug.LogError($"没有读取到{path}文件,请检查是否生成");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 重写文件
        /// 根据标识的范围重写
        /// </summary>
        /// <param name="path">精准路径 Assets/ ... XX.CS(扩展名)</param>
        /// <param name="valueDic">精准替换的范围与值</param>
        /// <param name="otherRetain">范围内的是否保留 默认保留 否则会覆盖</param>
        /// <returns></returns>
        public static bool OverrideCodeFile(string path, Dictionary<string, string> valueDic, bool otherRetain = true)
        {
            path = EditorHelper.GetProjPath(path);
            foreach (var pair in valueDic)
            {
                if (!RegionReplace(path, pair, otherRetain))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 指定区域内进行检查写入
        /// 需要区域内不存在才写入
        /// </summary>
        private static bool RegionCheckReplace2(string path, string signKey, string checkContent, string valueContent,
                                                bool   otherRetain = true)
        {
            //获取文件 
            if (path != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                StreamReader  streamReader  = new StreamReader(path);
                string        line          = streamReader.ReadLine();
                bool          checkExist    = false; //检查是否已经存在 如果已经存在则不重写 否则需要重写
                bool          isWrite       = false; //标记中途是否检查到已经写入过这个字段 防止重复
                string        startStr      = string.Format(m_StartSignFormat, signKey);
                string        endStr        = string.Format(m_EndSignFormat, signKey);

                while (line != null)
                {
                    if (line.IndexOf(startStr) > -1)
                    {
                        isWrite = true;
                        stringBuilder.Append(line + "\n");
                    }
                    else if (line.IndexOf(endStr) > -1)
                    {
                        if (isWrite)
                        {
                            if (!checkExist)
                            {
                                stringBuilder.Append($"{valueContent}\n");
                            }

                            isWrite = false;
                        }

                        stringBuilder.Append(line + "\n");
                    }
                    else
                    {
                        if (isWrite)
                        {
                            if (line.IndexOf(checkContent) > -1)
                            {
                                checkExist = true;
                            }

                            if (otherRetain)
                            {
                                stringBuilder.Append(line + "\n");
                            }
                        }
                        else
                        {
                            stringBuilder.Append(line + "\n");
                        }
                    }

                    line = streamReader.ReadLine();
                }

                streamReader.Close();
                StreamWriter streamWriter = new StreamWriter(path);
                streamWriter.Write(stringBuilder.ToString());
                streamWriter.Close();
            }
            else
            {
                Debug.LogError($"没有读取到{path}文件,请检查是否生成");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定区域内进行检查写入
        /// 需要区域内不存在才写入
        /// </summary>
        public static bool OverrideCheckCodeFile(string                                               path,
                                                 Dictionary<string, List<Dictionary<string, string>>> replaceDic,
                                                 bool                                                 cover = false)
        {
            var clsStr = RegionCheckReplace(path, replaceDic, cover);
            if (clsStr == null)
            {
                Debug.LogError("模板转化失败, path:" + path);
                return false;
            }

            try
            {
                path = EditorHelper.GetProjPath(path);
                string dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Debug.LogError("dir == null");
                    return false;
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, clsStr, Encoding.UTF8);
                Debug.Log(path + "重写成功");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("重写文件失败: path =" + path + ", err=" + e);
                return false;
            }
        }

        /// <summary>
        /// 指定区域内进行检查写入
        /// 需要区域内不存在才写入
        /// </summary>
        private static string RegionCheckReplace(string                                               path,
                                                 Dictionary<string, List<Dictionary<string, string>>> replaceDic,
                                                 bool                                                 cover = false)
        {
            var templateStr = File.ReadAllText(path);

            foreach (var item in replaceDic)
            {
                var key        = item.Key;
                var valueList  = item.Value;
                var startStr   = string.Format(m_StartSignFormat, key);
                var endStr     = string.Format(m_EndSignFormat, key);
                var startIndex = templateStr.IndexOf(startStr, StringComparison.Ordinal);
                var endIndex   = templateStr.IndexOf(endStr, StringComparison.Ordinal);
                if (startIndex <= -1 || endIndex <= startIndex)
                {
                    Debug.LogError($"{path} 此文件没有检查到关键字 {key} 请手动添加 关键字不允许移除");
                    continue;
                }

                var tempStr = templateStr.Substring(startIndex, endIndex - startIndex).Replace(" ", "");

                foreach (var data in valueList)
                {
                    foreach (var ovDic in data)
                    {
                        var check   = ovDic.Key.Replace(" ", "");
                        var content = ovDic.Value;
                        if (tempStr.IndexOf(check, StringComparison.Ordinal) <= -1)
                        {
                            if (cover)
                            {
                                //直接覆盖
                                templateStr = templateStr.Substring(0, startIndex + startStr.Length) + content + templateStr.Substring(endIndex);
                            }
                            else
                            {
                                //添加到最后
                                templateStr = templateStr.Insert(endIndex, content);
                            }
                        }
                    }
                }
            }

            return templateStr;
        }
    }
}