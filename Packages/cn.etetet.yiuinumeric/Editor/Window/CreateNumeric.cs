using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace YIUI.Numeric.Editor
{
    /// <summary>
    /// Numeric 生成器
    /// </summary>
    public class CreateNumeric
    {
        //数值源文件
        private const string NumericSourcePath = "Packages/cn.etetet.yiuinumericconfig/Assets/Editor/Luban/Other/NumericType.xlsx";

        //枚举导出位置
        private const string NumericEnumOutPath = "Packages/cn.etetet.yiuinumericconfig/Assets/Editor/Luban/Base/Defines";

        //检查导出位置
        private const string NumericCheckOutPath = "Packages/cn.etetet.yiuinumericconfig/Assets/Editor/Luban/Datas";

        private void Log(string content)
        {
            Debug.LogError(content);
        }

        private bool CheckConfigVersion()
        {
            var path             = $"{Application.dataPath}/../Packages";
            var configSourcePath = $"{path}/cn.etetet.yiuinumeric/configversions.txt";
            if (!File.Exists(configSourcePath))
            {
                Log($"没有找到配置文件 {configSourcePath}");
                return false;
            }

            var configSourceVersion = File.ReadAllText(configSourcePath);

            var configTargetPath = $"{path}/cn.etetet.yiuinumericconfig/configversions.txt";
            if (!File.Exists(configTargetPath))
            {
                Log($"没有找到配置文件 {configTargetPath}");
                return false;
            }

            var configTargetVersion = File.ReadAllText(configTargetPath);

            if (configSourceVersion != configTargetVersion)
            {
                Log($"配置文件版本不匹配 [{configSourceVersion}] != [{configTargetVersion}] 请更新配置文件\n 请参考文档:https://lib9kmxvq7k.feishu.cn/wiki/Sx86wDViniKzzVkhlUOc2s0VnEh");
                return false;
            }

            return true;
        }

        public bool Create()
        {
            if (!CheckConfigVersion()) return false;

            EditorUtility.DisplayProgressBar("数值", $"生成数据中...", 0);

            try
            {
                //Enum
                var enumContent = GetContent(NumericSourcePath);
                if (string.IsNullOrEmpty(enumContent))
                {
                    return false;
                }

                if (!WriteTextToProj($"{Application.dataPath}/../{NumericEnumOutPath}/numeric_enum.xml", enumContent))
                {
                    return false;
                }

                //Check
                var checkContent = GetValueTypeContent(NumericSourcePath);
                if (string.IsNullOrEmpty(checkContent))
                {
                    return false;
                }

                if (!WriteTextToProj($"{Application.dataPath}/../{NumericCheckOutPath}/NumericValueCheck.json", checkContent))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"生成导出数据失败 {e}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return false;
        }

        #region NumericTool

        readonly string TemplateXml = @"
<module name=""cn.etetet.yiuinumericconfig"" >
<enum name=""ENumericType"" >
{0}
</enum>
</module>";

        readonly string TemplateNumeric = @"
<!-- ${Desc} -->
<var name=""${Name}0"" value=""${Id0}""  alias=""${Alias}_0"" comment = ""${Alias}_0 (${ValueType})""/> 
<var name=""${Name}1"" value=""${Id1}"" alias=""${Alias}_1"" comment = ""${Alias}_1 (${ValueType})""/>
<var name=""${Name}2"" value=""${Id2}"" alias=""${Alias}_2"" comment = ""${Alias}_2 (${ValueType})""/>
<var name=""${Name}3"" value=""${Id3}"" alias=""${Alias}_3"" comment = ""${Alias}_3 (Float)""/>
<var name=""${Name}4"" value=""${Id4}"" alias=""${Alias}_4"" comment = ""${Alias}_4 (${ValueType})""/>
<var name=""${Name}5"" value=""${Id5}"" alias=""${Alias}_5"" comment = ""${Alias}_5 (Float)""/>
<var name=""${Name}6"" value=""${Id6}"" alias=""${Alias}_6"" comment = ""${Alias}_6 (${ValueType})""/>
";

        readonly string NotGrowTemplateNumeric = @"
<!-- ${Desc} -->
<var name=""${Name}0"" value=""${Id0}""  alias=""${Alias}_0"" comment = ""${Alias}_0 (${ValueType})""/> 
";

        /*
        string TemplateNumeric2 = @"
        comment=""0最终 ${Desc} 最终"" />
        comment=""1基础 ${Desc} 基础"" />
        comment=""2基础 ${Desc} 增加"" />
        comment=""3基础 ${Desc} 增加百分比"" />
        comment=""4基础 ${Desc} 最终增加"" />
        comment=""5基础 ${Desc} 最终增加百分比"" />
        comment=""6基础 ${Desc} 结果增加"" />
        ";
        */

        private const int Min = 100000;
        private const int Max = 1000000;

        private string GetContent(string excelPath)
        {
            var con = "";
            try
            {
                StringBuilder   Numeric  = new StringBuilder();
                HashSet<int>    allId    = new HashSet<int>();
                HashSet<string> allName  = new HashSet<string>();
                IWorkbook       book     = new XSSFWorkbook(new FileStream(excelPath, FileMode.Open));
                ISheet          sheet    = book.GetSheetAt(0);
                int             rowCount = sheet.LastRowNum + 1; //总行数 因为0开始他这里给-1了 下面的公式都是按+1计算的
                for (int i = 3; i < rowCount; i++)
                {
                    EditorUtility.DisplayProgressBar("数值", $"Enum 生成数据中...", i * 1f / rowCount);
                    IRow firstRow = sheet.GetRow(i); //每一行
                    if (firstRow == null)
                    {
                        Log($" 第{i + 1}行错误 没有读取到 请检查");
                        continue;
                    }

                    var GetCellStr = new Func<int, string>((int j) =>
                    {
                        var tempStr = "";
                        var cella   = firstRow.GetCell(j);
                        if (cella != null)
                        {
                            tempStr = cella.ToString();
                        }

                        return tempStr;
                    });

                    int.TryParse(GetCellStr(0), out int id); //ID
                    if (id is < Min or > Max)
                    {
                        if (id != 0) //不填=0 = 注释行
                        {
                            Log($" 第{i + 1}行错误 ID错误 范围{Min}-{Max} 请检查: {id}");
                        }

                        continue;
                    }

                    if (allId.Contains(id))
                    {
                        Log($" 第{i + 1}行错误 ID重复 请检查: {id}");
                        continue;
                    }

                    var name = GetCellStr(1); //名称
                    if (string.IsNullOrEmpty(name))
                    {
                        Log($" 第{i + 1}行错误 没有名字 请检查");
                        continue;
                    }

                    if (allName.Contains(name))
                    {
                        Log($" 第{i + 1}行错误 名字重复 请检查: {name}");
                        continue;
                    }

                    var alias     = GetCellStr(2);                        //别名
                    var valueType = GetCellStr(3);                        //类型
                    var notGrow   = !string.IsNullOrEmpty(GetCellStr(4)); //成长
                    if (valueType == "Bool")
                    {
                        notGrow = true;
                    }

                    var desc = GetCellStr(5); //描述
                    if (string.IsNullOrEmpty(desc))
                    {
                        desc = alias;
                    }

                    allId.Add(id);
                    allName.Add(name);
                    var tempFormat = notGrow ? NotGrowTemplateNumeric : TemplateNumeric;
                    var templateStr = tempFormat.Replace("${Name}", name)
                                                .Replace("${Id0}", id.ToString())
                                                .Replace("${Id1}", (id * 10 + 1).ToString())
                                                .Replace("${Id2}", (id * 10 + 2).ToString())
                                                .Replace("${Id3}", (id * 10 + 3).ToString())
                                                .Replace("${Id4}", (id * 10 + 4).ToString())
                                                .Replace("${Id5}", (id * 10 + 5).ToString())
                                                .Replace("${Id6}", (id * 10 + 6).ToString())
                                                .Replace("${Desc}", desc)
                                                .Replace("${Alias}", alias)
                                                .Replace("${ValueType}", valueType);

                    Numeric.Append(templateStr);
                }

                con = string.Format(TemplateXml, Numeric.ToString());
            }
            catch (Exception ex)
            {
                Log($"导出失败:(提示表打开的时候是无法导出的) 信息:\n{ex.Message}");
                return "";
            }

            return con;
        }

        readonly string TemplateValueTypeCS = @"
[
{0}
]";

        readonly string TemplateValueNumeric = @"
{""id"":""${Id0}"", ""check"":""${Check0}"",""alias"":""${Alias}"",""desc"":""${Desc}"",""not_grow"":false},
{""id"":""${Id1}"",""check"":""${Check1}"",""alias"":"""",""desc"":"""",""not_grow"":false},
{""id"":""${Id2}"",""check"":""${Check2}"",""alias"":"""",""desc"":"""",""not_grow"":false},
{""id"":""${Id3}"",""check"":""${Check3}"",""alias"":"""",""desc"":"""",""not_grow"":false},
{""id"":""${Id4}"",""check"":""${Check4}"",""alias"":"""",""desc"":"""",""not_grow"":false},
{""id"":""${Id5}"",""check"":""${Check5}"",""alias"":"""",""desc"":"""",""not_grow"":false},
{""id"":""${Id6}"",""check"":""${Check6}"",""alias"":"""",""desc"":"""",""not_grow"":false}";

        readonly string NotGrowTemplateValueNumeric = @"
{""id"":""${Id0}"", ""check"":""${Check0}"",""alias"":""${Alias}"",""desc"":""${Desc}"",""not_grow"":true}";

        private string GetValueTypeContent(string excelPath)
        {
            var con = "";
            try
            {
                StringBuilder   Numeric  = new StringBuilder();
                HashSet<int>    allId    = new HashSet<int>();
                HashSet<string> allName  = new HashSet<string>();
                IWorkbook       book     = new XSSFWorkbook(new FileStream(excelPath, FileMode.Open));
                ISheet          sheet    = book.GetSheetAt(0);
                int             rowCount = sheet.LastRowNum + 1; //总行数 因为0开始他这里给-1了 下面的公式都是按+1计算的
                for (int i = 3; i < rowCount; i++)
                {
                    EditorUtility.DisplayProgressBar("数值", $"Check 生成数据中...", i * 1f / rowCount);
                    IRow firstRow = sheet.GetRow(i); //每一行
                    if (firstRow == null)
                    {
                        Log($" 第{i + 1}行错误 没有读取到 请检查");
                        continue;
                    }

                    var GetCellStr = new Func<int, string>((int j) =>
                    {
                        var tempStr = "";
                        var cella   = firstRow.GetCell(j);
                        if (cella != null)
                        {
                            tempStr = cella.ToString();
                        }

                        return tempStr;
                    });

                    int.TryParse(GetCellStr(0), out int id); //ID
                    if (id is < Min or > Max)
                    {
                        if (id != 0) //不填=0 = 注释行
                        {
                            Log($" 第{i + 1}行错误 ID错误 范围{Min}-{Max} 请检查: {id}");
                        }

                        continue;
                    }

                    if (allId.Contains(id))
                    {
                        Log($" 第{i + 1}行错误 ID重复 请检查: {id}");
                        continue;
                    }

                    var name = GetCellStr(1); //名称
                    if (string.IsNullOrEmpty(name))
                    {
                        Log($" 第{i + 1}行错误 没有名字 请检查");
                        continue;
                    }

                    if (allName.Contains(name))
                    {
                        Log($" 第{i + 1}行错误 名字重复 请检查: {name}");
                        continue;
                    }

                    var alias = GetCellStr(2); //别名
                    var desc  = GetCellStr(5); //描述
                    if (string.IsNullOrEmpty(desc))
                    {
                        desc = alias;
                    }

                    var checkType = GetCellStr(3);                        //类型
                    var notGrow   = !string.IsNullOrEmpty(GetCellStr(4)); //成长
                    if (checkType == "Bool")
                    {
                        notGrow = true;
                    }

                    var tempFormat = notGrow ? NotGrowTemplateValueNumeric : TemplateValueNumeric;

                    allId.Add(id);
                    allName.Add(name);

                    var templateStr = tempFormat
                                      .Replace("${Alias}", alias)
                                      .Replace("${Desc}", desc)
                                      .Replace("${Id0}", id.ToString())
                                      .Replace("${Id1}", (id * 10 + 1).ToString())
                                      .Replace("${Id2}", (id * 10 + 2).ToString())
                                      .Replace("${Id3}", (id * 10 + 3).ToString())
                                      .Replace("${Id4}", (id * 10 + 4).ToString())
                                      .Replace("${Id5}", (id * 10 + 5).ToString())
                                      .Replace("${Id6}", (id * 10 + 6).ToString())
                                      .Replace("${Check0}", checkType)
                                      .Replace("${Check1}", checkType)
                                      .Replace("${Check2}", checkType)
                                      .Replace("${Check3}", "Float")
                                      .Replace("${Check4}", checkType)
                                      .Replace("${Check5}", "Float")
                                      .Replace("${Check6}", checkType);

                    Numeric.Append(templateStr);
                    if (i + 1 < rowCount)
                    {
                        Numeric.Append(",");
                    }
                }

                con = string.Format(TemplateValueTypeCS, Numeric.ToString());
            }
            catch (Exception ex)
            {
                Log($"导出失败:(提示表打开的时候是无法导出的) 信息:\n{ex.Message}");
                return "";
            }

            return con;
        }

        public bool WriteTextToProj(string path, string clsStr)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    return false;
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, clsStr);
                return true;
            }
            catch (Exception e)
            {
                Log($"写入错误:{e.Message}");
                return false;
            }
        }

        #endregion
    }
}