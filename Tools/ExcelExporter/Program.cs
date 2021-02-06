using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace ET
{
    public enum ConfigType
    {
        Client,
        Server,
    }
    
    struct HeadInfo
    {
        public string FieldAttribute;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;

        public HeadInfo(string cs, string desc, string name, string type)
        {
            this.FieldAttribute = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
        }
    }
    
    class Program
    {
        private static string template;

        private const string excelDir = "../../Excel";
        
        private const string classDir = "../../Generate/{0}/Code/Config";
        
        private const string jsonDir = "../../Generate/{0}/Json";
        
        static void Main(string[] args)
        {
            template = File.ReadAllText("Template.txt");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            foreach (string path in Directory.GetFiles(excelDir, "*.xlsx"))
            {
                using Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using ExcelPackage p = new ExcelPackage(stream);
                string name = Path.GetFileNameWithoutExtension(path);
                
                ExportExcelClass(p, name, ConfigType.Client);
                ExportExcelClass(p, name, ConfigType.Server);
                
                ExportExcelJson(p, name, ConfigType.Client);
                ExportExcelJson(p, name, ConfigType.Server);
            }
        }

        #region 导出class
        static void ExportExcelClass(ExcelPackage p, string name, ConfigType configType)
        {
            List<HeadInfo> classField = new List<HeadInfo>();
            HashSet<string> uniqeField = new HashSet<string>();
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheetClass(worksheet, classField, uniqeField, configType);
            }
            ExportClass(name, classField, configType);
        }
        
        static void ExportSheetClass(ExcelWorksheet worksheet, List<HeadInfo> classField, HashSet<string> uniqeField, ConfigType configType)
        {
            const int row = 2;
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }
                if (!uniqeField.Add(fieldName))
                {
                    continue;
                }
                string fieldCS = worksheet.Cells[row, col].Text.Trim();
                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                classField.Add(new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType));
            }
        }

        static void ExportClass(string protoName, List<HeadInfo> classField, ConfigType configType)
        {
            string exportPath = Path.Combine(string.Format(classDir, configType.ToString()), $"{protoName}.cs");
            
            using FileStream txt = new FileStream(exportPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < classField.Count; i++)
            {
                HeadInfo headInfo = classField[i];
                if (headInfo.FieldAttribute.StartsWith("#"))
                {
                    continue;
                }
                sb.Append($"\t\t[ProtoMember({i + 1}, IsRequired  = true)]\n");
                sb.Append($"\t\tpublic {headInfo.FieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }
            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }
        #endregion
        
        static void ExportExcelJson(ExcelPackage p, string name, ConfigType configType)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\"list\":[");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheetJson(worksheet, configType, sb);
            }
            sb.AppendLine("]}");
            
            string jsonPath = Path.Combine(string.Format(jsonDir, configType.ToString()), $"{name}.txt");
            using FileStream txt = new FileStream(jsonPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }
        
        static void ExportSheetJson(ExcelWorksheet worksheet, ConfigType configType, StringBuilder sb)
        {
            int infoRow = 2;
            List<HeadInfo> headInfos = new List<HeadInfo>();
            headInfos.Add(new HeadInfo());
            headInfos.Add(new HeadInfo());
            headInfos.Add(new HeadInfo());
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                string fieldName = worksheet.Cells[infoRow + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }
                string fieldCS = worksheet.Cells[infoRow, col].Text.Trim();
                string fieldDesc = worksheet.Cells[infoRow + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[infoRow + 3, col].Text.Trim();

                headInfos.Add(new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType));
            }
            
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                sb.Append("{");
                for (int col = 3; col < worksheet.Dimension.End.Column; ++col)
                {
                    HeadInfo headInfo = headInfos[col];
                    if (headInfo.FieldAttribute.Contains("#"))
                    {
                        continue;
                    }

                    if (headInfo.FieldName == "Id")
                    {
                        headInfo.FieldName = "_id";
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append($"\"{headInfo.FieldName}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }
                sb.Append("},\n");
            }
        }
        
        private static string Convert(string type, string value)
        {
            switch (type)
            {
                case "int[]":
                case "int32[]":
                case "long[]":
                    return $"[{value}]";
                case "string[]":
                    return $"[{value}]";
                case "int":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    return value;
                case "string":
                    return $"\"{value}\"";
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
        }
    }
}