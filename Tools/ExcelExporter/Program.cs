using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using OfficeOpenXml;
using ProtoBuf;
using LicenseContext = OfficeOpenXml.LicenseContext;

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

        private const string clientClassDir = "../../../Unity/Assets/Model/Generate/Config";
        private const string serverClassDir = "../../../Server/Model/Generate/Config";
        
        private const string excelDir = "../../../Excel";
        
        private const string jsonDir = "./{0}/Json";
        
        private const string clientProtoDir = "../../../Unity/Assets/Bundles/Config";
        private const string serverProtoDir = "../../../Config";

        private static string GetProtoDir(ConfigType configType)
        {
            if (configType == ConfigType.Client)
            {
                return clientProtoDir;
            }
            return serverProtoDir;
        }
        
        private static string GetClassDir(ConfigType configType)
        {
            if (configType == ConfigType.Client)
            {
                return clientClassDir;
            }
            return serverClassDir;
        }
        
        static void Main(string[] args)
        {
            try
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
            
                ExportExcelProtobuf(ConfigType.Client);
                ExportExcelProtobuf(ConfigType.Server);
                
                Console.WriteLine("导表成功!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
            string dir = GetClassDir(configType);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string exportPath = Path.Combine(dir, $"{protoName}.cs");
            
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

#region 导出json
        static void ExportExcelJson(ExcelPackage p, string name, ConfigType configType)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\"list\":[");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheetJson(worksheet, configType, sb);
            }
            sb.AppendLine("]}");
            
            string dir = string.Format(jsonDir, configType.ToString());
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            string jsonPath = Path.Combine(dir, $"{name}.txt");
            using FileStream txt = new FileStream(jsonPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }
        
        static void ExportSheetJson(ExcelWorksheet worksheet, ConfigType configType, StringBuilder sb)
        {
            int infoRow = 2;
            HeadInfo[] headInfos = new HeadInfo[100];
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                string fieldCS = worksheet.Cells[infoRow, col].Text.Trim();
                if (fieldCS.Contains("#"))
                {
                    continue;
                }
                
                string fieldName = worksheet.Cells[infoRow + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }
                
                string fieldDesc = worksheet.Cells[infoRow + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[infoRow + 3, col].Text.Trim();

                headInfos[col] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType);
            }
            
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                sb.Append("{");
                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
                {
                    HeadInfo headInfo = headInfos[col];
                    if (headInfo.FieldAttribute == null)
                    {
                        continue;
                    }
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
#endregion

        // 根据生成的类，动态编译把json转成protobuf
        private static void ExportExcelProtobuf(ConfigType configType)
        {
            string classPath = GetClassDir(configType);
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            List<string> protoNames = new List<string>();
            foreach (string classFile in Directory.GetFiles(classPath, "*.cs"))
            {
                protoNames.Add(Path.GetFileNameWithoutExtension(classFile));
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(classFile)));
            }
            
            List<PortableExecutableReference> references = new List<PortableExecutableReference>();
            
            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            references.Add(AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(typeof(ProtoMemberAttribute).Assembly.Location).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(typeof(BsonDefaultValueAttribute).Assembly.Location).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(typeof(IConfig).Assembly.Location).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Attribute).Assembly.Location).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(Path.Combine(assemblyPath, "System.dll")).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")).GetReference());
            references.Add(AssemblyMetadata.CreateFromFile(typeof(ISupportInitialize).Assembly.Location).GetReference());
           
            
            CSharpCompilation compilation = CSharpCompilation.Create(
                null, 
                syntaxTrees.ToArray(), 
                references.ToArray(), 
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream memSteam = new MemoryStream();
            
            EmitResult emitResult = compilation.Emit(memSteam);
            if (!emitResult.Success)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Diagnostic t in emitResult.Diagnostics)
                {
                    stringBuilder.AppendLine(t.GetMessage());
                }
                throw new Exception($"动态编译失败:\n{stringBuilder}");
            }
            
            memSteam.Seek(0, SeekOrigin.Begin);

            Assembly ass = Assembly.Load(memSteam.ToArray());

            string dir = GetProtoDir(configType);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            foreach (string protoName in protoNames)
            {
                Type type = ass.GetType($"ET.{protoName}Category");
                Type subType = ass.GetType($"ET.{protoName}");
                Serializer.NonGeneric.PrepareSerializer(type);
                Serializer.NonGeneric.PrepareSerializer(subType);
                
                
                string json = File.ReadAllText(Path.Combine(string.Format(jsonDir, configType), $"{protoName}.txt"));
                object deserialize = BsonSerializer.Deserialize(json, type);

                string path = Path.Combine(dir, $"{protoName}Category.bytes");

                using FileStream file = File.Create(path);
                Serializer.Serialize(file, deserialize);
            }
        }
    }
}