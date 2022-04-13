using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson.Serialization;
using OfficeOpenXml;
using ProtoBuf;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ET
{
    public enum ConfigType
    {
        c = 0,
        s = 1,
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

    // 这里加个标签是为了防止编译时裁剪掉protobuf，因为整个tool工程没有用到protobuf，编译会去掉引用，然后动态编译就会出错
    [ProtoContract]
    class Table
    {
        public bool C;
        public bool S;
        public int Index;
        public Dictionary<string, HeadInfo> HeadInfos = new Dictionary<string, HeadInfo>();
    }
    
    public static class ExcelExporter
    {
        private static string template;

        public const string ClientClassDir = "../Unity/Codes/Model/Generate/Config";
        public const string ServerClassDir = "../Server/Model/Generate/Config";

        private const string excelDir = "../Excel";

        private const string jsonDir = "../Excel/Json/{0}/{1}";

        private const string clientProtoDir = "../Unity/Assets/Bundles/Config/{0}";
        private const string serverProtoDir = "../Config/{0}";
        private static Assembly[] configAssemblies = new Assembly[2];

        private static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        private static Dictionary<string, ExcelPackage> packages = new Dictionary<string, ExcelPackage>();

        private static Table GetTable(string protoName)
        {
            if (!tables.TryGetValue(protoName, out var table))
            {
                table = new Table();
                tables[protoName] = table;
            }

            return table;
        }

        public static ExcelPackage GetPackage(string filePath)
        {
            if (!packages.TryGetValue(filePath, out var package))
            {
                using Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                package = new ExcelPackage(stream);
                packages[filePath] = package;
            }

            return package;
        }

        public static void Export()
        {
            try
            {
                template = File.ReadAllText("Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                if (Directory.Exists(ClientClassDir))
                {
                    Directory.Delete(ClientClassDir, true);
                }

                if (Directory.Exists(ServerClassDir))
                {
                    Directory.Delete(ServerClassDir, true);
                }

                foreach (string path in Directory.GetFiles(excelDir))
                {
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                    {
                        continue;
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string fileNameWithoutCS = fileNameWithoutExtension;
                    string cs = "cs";
                    if (fileNameWithoutExtension.Contains("@"))
                    {
                        string[] ss = fileNameWithoutExtension.Split("@");
                        fileNameWithoutCS = ss[0];
                        cs = ss[1];
                    }

                    if (cs == "")
                    {
                        cs = "cs";
                    }

                    ExcelPackage p = GetPackage(Path.GetFullPath(path));

                    string protoName = fileNameWithoutCS;
                    if (fileNameWithoutCS.Contains('_'))
                    {
                        protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
                    }

                    Table table = GetTable(protoName);

                    if (cs.Contains("c"))
                    {
                        table.C = true;
                    }

                    if (cs.Contains("s"))
                    {
                        table.S = true;
                    }

                    ExportExcelClass(p, protoName, table);
                }

                foreach (var kv in tables)
                {
                    if (kv.Value.C)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c);
                    }

                    if (kv.Value.S)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.s);
                    }
                }

                // 动态编译生成的配置代码
                configAssemblies[(int) ConfigType.c] = DynamicBuild(ConfigType.c);
                configAssemblies[(int) ConfigType.s] = DynamicBuild(ConfigType.s);

                foreach (string path in Directory.GetFiles(excelDir))
                {
                    ExportExcel(path);
                }
                
                // 多线程导出
                //List<Task> tasks = new List<Task>();
                //foreach (string path in Directory.GetFiles(excelDir))
                //{
                //    Task task = Task.Run(() => ExportExcel(path));
                //    tasks.Add(task);
                //}
                //Task.WaitAll(tasks.ToArray());

                // 导出StartConfig
                string startConfigPath = Path.Combine(excelDir, "StartConfig");
                DirectoryInfo directoryInfo = new DirectoryInfo(startConfigPath);
                foreach (FileInfo subStartConfig in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    ExportExcel(subStartConfig.FullName);
                }

                Log.Console("Export Excel Sucess!");
            }
            catch (Exception e)
            {
                Log.Console(e.ToString());
            }
            finally
            {
                tables.Clear();
                foreach (var kv in packages)
                {
                    kv.Value.Dispose();
                }

                packages.Clear();
            }
        }

        private static void ExportExcel(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string relativePath = Path.GetRelativePath(excelDir, dir);
            string fileName = Path.GetFileName(path);
            if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithoutCS = fileNameWithoutExtension;
            string cs = "cs";
            if (fileNameWithoutExtension.Contains("@"))
            {
                string[] ss = fileNameWithoutExtension.Split("@");
                fileNameWithoutCS = ss[0];
                cs = ss[1];
            }

            if (cs == "")
            {
                cs = "cs";
            }

            string protoName = fileNameWithoutCS;
            if (fileNameWithoutCS.Contains('_'))
            {
                protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
            }

            Table table = GetTable(protoName);

            ExcelPackage p = GetPackage(Path.GetFullPath(path));

            if (cs.Contains("c"))
            {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.c, relativePath);
                ExportExcelProtobuf(ConfigType.c, protoName, relativePath);
            }

            if (cs.Contains("s"))
            {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.s, relativePath);
                ExportExcelProtobuf(ConfigType.s, protoName, relativePath);
            }
        }

        private static string GetProtoDir(ConfigType configType, string relativeDir)
        {
            if (configType == ConfigType.c)
            {
                return string.Format(clientProtoDir, relativeDir);
            }

            return string.Format(serverProtoDir, relativeDir);
        }

        private static Assembly GetAssembly(ConfigType configType)
        {
            return configAssemblies[(int) configType];
        }

        private static string GetClassDir(ConfigType configType)
        {
            if (configType == ConfigType.c)
            {
                return ClientClassDir;
            }

            return ServerClassDir;
        }
        
        // 动态编译生成的cs代码
        private static Assembly DynamicBuild(ConfigType configType)
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

            CSharpCompilation compilation = CSharpCompilation.Create(null,
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
            return ass;
        }


        #region 导出class

        static void ExportExcelClass(ExcelPackage p, string name, Table table)
        {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheetClass(worksheet, table);
            }
        }

        static void ExportSheetClass(ExcelWorksheet worksheet, Table table)
        {
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

                if (table.HeadInfos.ContainsKey(fieldName))
                {
                    continue;
                }

                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains("#"))
                {
                    table.HeadInfos[fieldName] = null;
                    continue;
                }

                if (fieldCS == "")
                {
                    fieldCS = "cs";
                }

                if (table.HeadInfos.TryGetValue(fieldName, out var oldClassField))
                {
                    if (oldClassField.FieldAttribute != fieldCS)
                    {
                        Log.Console($"field cs not same: {worksheet.Name} {fieldName} oldcs: {oldClassField.FieldAttribute} {fieldCS}");
                    }

                    continue;
                }

                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                table.HeadInfos[fieldName] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType, ++table.Index);
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

                if (headInfo.FieldType == "json")
                {
                    continue;
                }

                if (!headInfo.FieldAttribute.Contains(configType.ToString()))
                {
                    continue;
                }

                sb.Append($"\t\t/// <summary>{headInfo.FieldDesc}</summary>\n");
                sb.Append($"\t\t[ProtoMember({headInfo.FieldIndex})]\n");
                string fieldType = headInfo.FieldType;
                if (fieldType == "int[][]")
                {
                    fieldType = "string[]";
                }

                sb.Append($"\t\tpublic {fieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }

            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }

        #endregion

        #region 导出json


        static void ExportExcelJson(ExcelPackage p, string name, Table table, ConfigType configType, string relativeDir)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\"list\":[");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }

                ExportSheetJson(worksheet, name, table.HeadInfos, configType, sb);
            }

            sb.AppendLine("]}");

            string dir = string.Format(jsonDir, configType.ToString(), relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string jsonPath = Path.Combine(dir, $"{name}.txt");
            using FileStream txt = new FileStream(jsonPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }

        static void ExportSheetJson(ExcelWorksheet worksheet, string name, 
                Dictionary<string, HeadInfo> classField, ConfigType configType, StringBuilder sb)
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

                    if (headInfo.FieldType == "json")
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
                case "uint[]":
                case "int[]":
                case "int32[]":
                case "long[]":
                    {
                        value = value.Replace("{", "").Replace("}", "");
                        return $"[{value}]";
                    }
                case "string[]":
                case "int[][]":
                    return $"[{value}]";
                case "int":
                case "uint":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    {
                        value = value.Replace("{", "").Replace("}", "");
                        if (value == "")
                        {
                            return "0";
                        }
                        return value;
                    }
                case "string":
                    return $"\"{value}\"";
                case "AttrConfig":
                    string[] ss = value.Split(':');
                    return "{\"_t\":\"AttrConfig\"," + "\"Ks\":" + ss[0] + ",\"Vs\":" + ss[1] + "}";
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
        }

        #endregion


        // 根据生成的类，把json转成protobuf
        private static void ExportExcelProtobuf(ConfigType configType, string protoName, string relativeDir)
        {
            string dir = GetProtoDir(configType, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Assembly ass = GetAssembly(configType);
            Type type = ass.GetType($"ET.{protoName}Category");
            Type subType = ass.GetType($"ET.{protoName}");

            Serializer.NonGeneric.PrepareSerializer(type);
            Serializer.NonGeneric.PrepareSerializer(subType);

            IMerge final = Activator.CreateInstance(type) as IMerge;

            string p = Path.Combine(string.Format(jsonDir, configType, relativeDir));
            string[] ss = Directory.GetFiles(p, $"{protoName}_*.txt");
            List<string> jsonPaths = ss.ToList();
            jsonPaths.Add(Path.Combine(string.Format(jsonDir, configType, relativeDir), $"{protoName}.txt"));

            jsonPaths.Sort();
            jsonPaths.Reverse();
            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                object deserialize = BsonSerializer.Deserialize(json, type);
                final.Merge(deserialize);
            }

            string path = Path.Combine(dir, $"{protoName}Category.bytes");

            using FileStream file = File.Create(path);
            Serializer.Serialize(file, final);
        }
    }
}