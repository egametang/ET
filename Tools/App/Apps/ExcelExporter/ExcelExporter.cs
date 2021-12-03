using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using OfficeOpenXml;
using ProtoBuf;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ET
{
    public enum ConfigType
    {
        c,
        s,
    }
    
    class HeadInfo
    {
        public string FieldAttribute;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;
        public int FieldIndex;

        public HeadInfo(string cs, string desc, string name, string type, int index)
        {
            this.FieldAttribute = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
            this.FieldIndex = index;
        }
    }
    
    public static class ExcelExporter
    {
        private static string template;

        private const string clientClassDir = "../Unity/Codes/Model/Generate/Config";
        private const string serverClassDir = "../Server/Model/Generate/Config";
        
        private const string excelDir = "../Excel";
        
        private const string jsonDir = "./Json/{0}";
        
        private const string clientProtoDir = "../Unity/Assets/Bundles/Config";
        private const string serverProtoDir = "../Config";
        
        public static void Export()
        {
            try
            {
                template = File.ReadAllText("Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                if (Directory.Exists(clientClassDir))
                {
                    Directory.Delete(clientClassDir, true);
                }

                if (Directory.Exists(serverClassDir))
                {
                    Directory.Delete(serverClassDir, true);
                }

                foreach (string path in Directory.GetFiles(excelDir))
                {
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$"))
                    {
                        continue;
                    }
                    using Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using ExcelPackage p = new ExcelPackage(stream);
                    string name = Path.GetFileNameWithoutExtension(path);

                    string cs = p.Workbook.Worksheets[0].Cells[1, 1].Text.Trim();
                    if (cs.Contains("#"))
                    {
                        continue;
                    }
                    if (cs == "")
                    {
                        cs = "cs";
                    }
                    if (cs.Contains("c"))
                    {
                        ExportExcelClass(p, name, ConfigType.c);    
                    }
                    if (cs.Contains("s"))
                    {
                        ExportExcelClass(p, name, ConfigType.s);    
                    }
                }
            
                ExportExcelProtobuf(ConfigType.c);
                ExportExcelProtobuf(ConfigType.s);
                
                Console.WriteLine("导表成功!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string GetProtoDir(ConfigType configType)
        {
            if (configType == ConfigType.c)
            {
                return clientProtoDir;
            }
            return serverProtoDir;
        }
        
        private static string GetClassDir(ConfigType configType)
        {
            if (configType == ConfigType.c)
            {
                return clientClassDir;
            }
            return serverClassDir;
        }
        
        
#region 导出class
        static void ExportExcelClass(ExcelPackage p, string name, ConfigType configType)
        {
            Dictionary<string, HeadInfo> classField = new Dictionary<string, HeadInfo>();
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheetClass(worksheet, classField, configType);
            }
            ExportClass(name, classField, configType);
            ExportExcelJson(p, name, classField, configType);
        }
        
        static void ExportSheetClass(ExcelWorksheet worksheet, Dictionary<string, HeadInfo> classField, ConfigType configType)
        {
            string configTypeStr = configType.ToString();
            const int row = 2;
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }
                
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }

                if (classField.ContainsKey(fieldName))
                {
                    continue;
                }
                
                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains("#"))
                {
                    classField[fieldName] = null;
                    continue;
                }

                if (fieldCS == "")
                {
                    fieldCS = "cs";
                }

                if (!fieldCS.Contains(configTypeStr))
                {
                    classField[fieldName] = null;
                    continue;
                }
                
                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                classField[fieldName] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType, col);
            }
        }

        static void ExportClass(string protoName, Dictionary<string, HeadInfo> classField, ConfigType configType)
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
            foreach ((string _, HeadInfo headInfo) in classField)
            {
                if (headInfo == null)
                {
                    continue;
                }
                sb.Append($"\t\t[ProtoMember({headInfo.FieldIndex - 2})]\n");
                sb.Append($"\t\tpublic {headInfo.FieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }
            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }
#endregion

#region 导出json
        static void ExportExcelJson(ExcelPackage p, string name, Dictionary<string, HeadInfo> classField, ConfigType configType)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\"list\":[");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }
                ExportSheetJson(worksheet, name, classField, configType, sb);
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
        
        static void ExportSheetJson(ExcelWorksheet worksheet, string name, Dictionary<string, HeadInfo> classField, ConfigType configType, StringBuilder sb)
        {
            string configTypeStr = configType.ToString();
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#"))
                {
                    continue;
                }

                if (prefix == "")
                {
                    prefix = "cs";
                }

                if (!prefix.Contains(configTypeStr))
                {
                    continue;
                }
                
                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }
                sb.Append("{");
                sb.Append($"\"_t\":\"{name}\"");
                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
                {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!classField.ContainsKey(fieldName))
                    {
                        continue;
                    }

                    HeadInfo headInfo = classField[fieldName];

                    if (headInfo == null)
                    {
                        continue;
                    }
                    
                    string fieldN = headInfo.FieldName;
                    if (fieldN == "Id")
                    {
                        fieldN = "_id";
                    }
                    sb.Append($",\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
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
                    if (value == "")
                    {
                        return "0";
                    }
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
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }

                    if (assembly.Location == "")
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location);
                references.Add(reference);
            }

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