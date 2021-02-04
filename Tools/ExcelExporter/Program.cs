using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace ET
{
    struct HeadInfo
    {
        public string ClientServer;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;

        public HeadInfo(string cs, string desc, string name, string type)
        {
            this.ClientServer = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
        }
    }
    
    class Program
    {
        private static string template;
        
        static void Main(string[] args)
        {
            template = File.ReadAllText("Template.txt");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExportExcel("/Users/tanghai/Documents/ET/Excel/StartZoneConfig.xlsx", new StringBuilder());
        }

        static void ExportExcel(string path, StringBuilder stringBuilder)
        {
            using Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using ExcelPackage p = new ExcelPackage(stream);

            List<HeadInfo> classField = new List<HeadInfo>();
            HashSet<string> uniqeField = new HashSet<string>();
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheet(worksheet, classField, uniqeField);
            }

            ExportClass(Path.GetFileNameWithoutExtension(path), classField, "./");
        }
        
        static void ExportSheet(ExcelWorksheet worksheet, List<HeadInfo> classField, HashSet<string> uniqeField)
        {
            const int row = 2;
            for (int col = 3; col < worksheet.Dimension.Columns; ++col)
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

        static void ExportClass(string protoName, List<HeadInfo> classField, string exportDir)
        {
            string exportPath = Path.Combine(exportDir, $"{protoName}.cs");
            
            using FileStream txt = new FileStream(exportPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < classField.Count; i++)
            {
                HeadInfo headInfo = classField[i];
                if (headInfo.ClientServer.StartsWith("#"))
                {
                    continue;
                }
                sb.Append($"\t\t[ProtoMember({i + 1}, IsRequired  = true)]\n");
                sb.Append($"\t\tpublic {headInfo.FieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }
            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }
    }
}