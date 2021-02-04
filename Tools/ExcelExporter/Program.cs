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
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExportExcel("/Users/tanghai/Documents/ET/Excel/StartMachineConfig.xlsx", new StringBuilder());
        }

        static void ExportExcel(string path, StringBuilder stringBuilder)
        {
            FileInfo fileInfo = new FileInfo(path);
            using ExcelPackage p = new ExcelPackage(fileInfo);

            List<HeadInfo> classField = new List<HeadInfo>();
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheet(worksheet, classField);
            }

            ExportClass(Path.GetFileNameWithoutExtension(path), classField, "");

            foreach (HeadInfo headInfo in classField)
            {
                Console.WriteLine($"{headInfo.FieldName} {headInfo.FieldType} {headInfo.FieldDesc}");
            }
        }
        
        static void ExportSheet(ExcelWorksheet worksheet, List<HeadInfo> classField)
        {
            const int row = 2;
            for (int col = 3; col < worksheet.Dimension.Columns; ++col)
            {
                string fieldCS = worksheet.Cells[row, col].Text.Trim();
                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }

                classField.Add(new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType));
            }
        }

        static void ExportClass(string protoName, List<HeadInfo> classField, string exportDir)
        {
            string exportPath = Path.Combine(exportDir, $"{protoName}.cs");
            using (FileStream txt = new FileStream(exportPath, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(txt))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("namespace ET\n{");
                sb.Append($"\t[Config]\n");
                sb.Append($"\tpublic partial class {protoName}Category : ACategory<{protoName}>\n");
                sb.Append("\t{\n");
                sb.Append($"\t\tpublic static {protoName}Category Instance;\n");
                sb.Append($"\t\tpublic {protoName}Category()\n");
                sb.Append("\t\t{\n");
                sb.Append($"\t\t\tInstance = this;\n");
                sb.Append("\t\t}\n");
                sb.Append("\t}\n\n");

                sb.Append($"\tpublic partial class {protoName}: IConfig\n");
                sb.Append("\t{\n");
                sb.Append("\t\t[BsonId]\n");
                sb.Append("\t\tpublic long Id { get; set; }\n");

                for (int i = 0; i < classField.Count; i++)
                {
                    HeadInfo headInfo = classField[i];
                    if (headInfo.ClientServer.StartsWith("#"))
                    {
                        continue;
                    }
                    sb.Append($"\t\tpublic {headInfo.FieldType} {headInfo.FieldName} {{ get; set;}};\n");
                }

                sb.Append("\t}\n");
                sb.Append("}\n");

                sw.Write(sb.ToString());
            }
        }
    }
}