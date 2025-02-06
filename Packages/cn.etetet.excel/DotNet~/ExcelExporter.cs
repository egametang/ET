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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ET
{
    public enum ConfigType
    {
        c = 0,
        s = 1,
        cs = 2,
    }

    class HeadInfo
    {
        [BsonElement]
        public string FieldCS;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;
        public int FieldIndex;

        public HeadInfo(string cs, string desc, string name, string type, int index)
        {
            this.FieldCS = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
            this.FieldIndex = index;
        }
    }

    // 这里加个标签是为了防止编译时裁剪掉protobuf，因为整个tool工程没有用到protobuf，编译会去掉引用，然后动态编译就会出错
    class Table
    {
        public string Name;
        public bool C;
        public bool S;
        public int Index;
        public Dictionary<string, HeadInfo> HeadInfos = new();
    }
    
    [EnableClass]
    public static class ExcelExporter
    {
        private static string template;

        private static string ClientClassDir;
        // 服务端因为机器人的存在必须包含客户端所有配置，所以单独的c字段没有意义,单独的c就表示cs
        private static string ServerClassDir;
        private static string CSClassDir;

        private const string jsonDir = "./Packages/cn.etetet.excel/Config/Json";

        private const string serverProtoDir = "./Packages/cn.etetet.excel/Config/Bytes";
        private static Assembly[] configAssemblies = new Assembly[3];

        private static Dictionary<string, Table> tables = new();
        private static Dictionary<string, ExcelPackage> packages = new();

        private static Table GetTable(string protoName)
        {
            string fullName = protoName;
            if (!tables.TryGetValue(fullName, out var table))
            {
                table = new Table();
                table.Name = protoName;
                tables[fullName] = table;
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
                // 强制调用一下mongo，避免mongo库被裁剪
                MongoHelper.ToJson(1);
                
                template = File.ReadAllText("./Packages/cn.etetet.excel/DotNet~/Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                PackagesLock packagesLock = PackageHelper.LoadEtPackagesLock("./");
                PackageInfo excelPackage = packagesLock.dependencies["cn.etetet.excel"];
                ClientClassDir = Path.Combine(excelPackage.dir, "CodeMode/Model/Client");
                ServerClassDir = Path.Combine(excelPackage.dir, "CodeMode/Model/Server");
                CSClassDir = Path.Combine(excelPackage.dir, "CodeMode/Model/ClientServer");
            
                if (Directory.Exists(jsonDir))
                {
                    Directory.Delete(jsonDir, true);
                }

                if (Directory.Exists(serverProtoDir))
                {
                    Directory.Delete(serverProtoDir, true);
                }
                
                List<string> list = new();
                foreach ((string key, PackageInfo packageInfo) in packagesLock.dependencies)
                {
                    string p = Path.Combine(packageInfo.dir, "Excel");
                    if (!Directory.Exists(p))
                    {
                        continue;
                    }
                    list.Add(p);
                }

                List<(string, string)> paths = new();
                foreach (string s in list)
                {
                    var aa = FileHelper.GetAllFiles(s);
                    
                    foreach (string k in aa)
                    {
                        if (k.EndsWith(".xlsx") || k.EndsWith(".xlsm"))
                        {
                            paths.Add((s, k));
                        }
                    }
                }
                
                foreach ((string s, string path) in paths)
                {
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                    {
                        continue;
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    
                    //对Excel名称进行拆分, 移除掉最后一个_后面的文本内容 
                    if(fileNameWithoutExtension.Contains('_'))
                    {
                        fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.LastIndexOf('_'));
                    }

                    string fileNameWithoutCS = fileNameWithoutExtension;
                    string cs = "cs";
                    if (fileNameWithoutExtension.Contains('@'))
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
                        ExportClass(kv.Value, ConfigType.c);
                    }
                    if (kv.Value.S)
                    {
                        ExportClass(kv.Value, ConfigType.s);
                    }
                    ExportClass(kv.Value, ConfigType.cs);
                }

                // 动态编译生成的配置代码
                configAssemblies[(int) ConfigType.c] = DynamicBuild(ConfigType.c);
                configAssemblies[(int) ConfigType.s] = DynamicBuild(ConfigType.s);
                configAssemblies[(int) ConfigType.cs] = DynamicBuild(ConfigType.cs);
                
                foreach ((string s, string path) in paths)
                {
                    ExportExcel(s, path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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

        private static void ExportExcel(string root, string path)
        {
            string dir = Path.GetDirectoryName(path);
            string relativePath = Path.GetRelativePath(root, dir);
            string fileName = Path.GetFileName(path);
            if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
            //对Excel名称进行拆分, 移除掉最后一个_后面的文本内容 
            if(fileNameWithoutExtension.Contains('_'))
            {
                fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.LastIndexOf('_'));
            }
            
            string fileNameWithoutCS = fileNameWithoutExtension;
            string cs = "cs";
            if (fileNameWithoutExtension.Contains('@'))
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
                ExportExcelProtobuf(ConfigType.c, table, relativePath);
            }

            if (cs.Contains("s"))
            {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.s, relativePath);
                ExportExcelProtobuf(ConfigType.s, table, relativePath);
            }
            ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.cs, relativePath);
            ExportExcelProtobuf(ConfigType.cs, table, relativePath);
        }

        private static string GetProtoDir(ConfigType configType, string relativeDir)
        {
            return Path.Combine(serverProtoDir, configType.ToString(), relativeDir);
        }

        private static Assembly GetAssembly(ConfigType configType)
        {
            return configAssemblies[(int) configType];
        }

        private static string GetClassDir(ConfigType configType)
        {
            return configType switch
            {
                ConfigType.c => ClientClassDir,
                ConfigType.s => ServerClassDir,
                _ => CSClassDir
            };
        }
        
        // 动态编译生成的cs代码
        private static Assembly DynamicBuild(ConfigType configType)
        {
            string classPath = GetClassDir(configType);
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            List<string> protoNames = new List<string>();
            foreach (string classFile in FileHelper.GetAllFiles(classPath, "*.cs"))
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
                    stringBuilder.Append($"{t.GetMessage()}\n");
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
                string sheetName = worksheet.Name.ToLower();
                if (sheetName.StartsWith("#const_"))
                {
                    ExportConstClass(worksheet);
                    continue;
                }
                if (sheetName.StartsWith("#enum_"))
                {
                    ExportEnumClass(worksheet);
                    continue;
                }

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
                    if (oldClassField.FieldCS != fieldCS)
                    {
                        Console.WriteLine($"field cs not same: {worksheet.Name} {fieldName} oldcs: {oldClassField.FieldCS} {fieldCS}");
                    }

                    continue;
                }

                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                table.HeadInfos[fieldName] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType, ++table.Index);
            }
        }

        static void ExportClass(Table table, ConfigType configType)
        {
            string dir = GetClassDir(configType);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string exportPath = Path.Combine(dir, $"{table.Name}.cs");

            if (!Directory.Exists(Path.GetDirectoryName(exportPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
            }
            
            using FileStream txt = new FileStream(exportPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);

            StringBuilder sb = new StringBuilder();
            foreach ((string _, HeadInfo headInfo) in table.HeadInfos)
            {
                if (headInfo == null)
                {
                    continue;
                }

                if (configType != ConfigType.cs && !headInfo.FieldCS.Contains(configType.ToString()))
                {
                    continue;
                }

                sb.Append($"\t\t/// <summary>{headInfo.FieldDesc}</summary>\n");
                string fieldType = headInfo.FieldType;
                sb.Append($"\t\tpublic {fieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }

            //template = template.Replace("(ns)", $"ET.{table.Module}");
            template = template.Replace("(ns)", "ET");
            string content = template.Replace("(ConfigName)", table.Name).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }

        #endregion

        #region 导出Const 和 Enum

        // 导出常量数据
        static void ExportConstClass(ExcelWorksheet worksheet)
        {
            const int row = 2;
            List<string> listConst = new List<string>();
            
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }

                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains('#'))
                {
                    continue;
                }
                
                if (fieldCS == "")
                {
                    fieldCS = "cs";
                }

                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                if (fieldType.ToLower() == "const")
                {
                    string constType = worksheet.Cells[row + 3, col + 1].Text.Trim();
                    
                    //数组类型 ,需要使用static readonly
                    bool isStatic = constType.Contains("[]");
                    
                    for (int i = 0; i < 999999; i++)
                    {
                        string name = worksheet.Cells[row + 4 + i, col].Text.Trim();
                        if(string.IsNullOrEmpty(name)) break;
                        
                        string desc = worksheet.Cells[row + 4 + i, col - 1].Text.Trim();
                        string val = worksheet.Cells[row + 4 + i, col + 1].Text.Trim();

                        if (isStatic)
                        {
                            //数组类型,需要使用{}包裹
                            val = "{" + val + "}";
                            listConst.Add($"        /// <summary>{desc}</summary>\n        [StaticField]\n        public static readonly {constType} {name} = {val}; \n");
                        }
                        else
                        {
                            listConst.Add($"        /// <summary>{desc}</summary>\n        public const {constType} {name} = {Convert(constType, val)}; \n");
                        }
                    }
                }
            }
            
            
            string cs = worksheet.Cells[1, 1].Text.Trim();
            
            List<ConfigType> listTypes = new List<ConfigType>() { ConfigType.cs , ConfigType.c, ConfigType.s };
            if (cs == "c")
            {
                listTypes.Remove(ConfigType.s);
            }
            else if (cs == "s")
            {
                listTypes.Remove(ConfigType.c);
            }

            foreach (var configType in listTypes)
            {
                string dir = GetClassDir(configType);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
    
                string ename = worksheet.Name.Substring(7); // #const_ 7个字符
                string exportPath = Path.Combine(dir, $"{ename}.cs");
    
                using FileStream txt = new FileStream(exportPath, FileMode.Create);
                using StreamWriter sw = new StreamWriter(txt);
    
                //生成常量
                sw.WriteLine("namespace ET");
                sw.WriteLine("{");
                sw.WriteLine($"    public static partial class {ename}");
                sw.WriteLine("    {");
                for(int i = 0 ; i < listConst.Count ; i++)
                {
                    sw.WriteLine(listConst[i]);
                }
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }
        }
        
        
        // 导出枚举数据
        static void ExportEnumClass(ExcelWorksheet worksheet)
        {
            const int row = 2;
            List<string> listEnums = new List<string>();
            
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }

                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains('#'))
                {
                    continue;
                }
                
                if (fieldCS == "")
                {
                    fieldCS = "cs";
                }

                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                if (fieldType.ToLower() == "enum")
                {
                    for (int i = 0; i < 999999; i++)
                    {
                        string name = worksheet.Cells[row + 4 + i, col].Text.Trim();
                        if(string.IsNullOrEmpty(name)) break;
                        
                        string desc = worksheet.Cells[row + 4 + i, col - 1].Text.Trim();
                        string val = worksheet.Cells[row + 4 + i, col + 1].Text.Trim();

                        if (string.IsNullOrEmpty(val)) 
                            val = ",";
                        else
                        {
                            val = $" = {val},";
                        }
                        
                        listEnums.Add($"        /// <summary>{desc}</summary>\n        {name}{val}\n");
                    }
                }
            }
            
            
            string cs = worksheet.Cells[1, 1].Text.Trim();
            
            List<ConfigType> listTypes = new List<ConfigType>() { ConfigType.cs , ConfigType.c, ConfigType.s };
            if (cs == "c")
            {
                listTypes.Remove(ConfigType.s);
            }
            else if (cs == "s")
            {
                listTypes.Remove(ConfigType.c);
            }

            foreach (var configType in listTypes)
            {
                string dir = GetClassDir(configType);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
    
                string ename = worksheet.Name.Substring(6); // #enum_ 6个字符
                string exportPath = Path.Combine(dir, $"{ename}.cs");
    
                using FileStream txt = new FileStream(exportPath, FileMode.Create);
                using StreamWriter sw = new StreamWriter(txt);
    
                //生成枚举
                sw.WriteLine("namespace ET");
                sw.WriteLine("{");
                sw.WriteLine($"    public enum {ename}");
                sw.WriteLine("    {");
                for(int i = 0 ; i < listEnums.Count ; i++)
                {
                    sw.WriteLine(listEnums[i]);
                }
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }
        }
        

        #endregion

        #region 导出json


        static void ExportExcelJson(ExcelPackage p, string name, Table table, ConfigType configType, string relativeDir)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"dict\": [\n");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }

                ExportSheetJson(worksheet, name, table, configType, sb);
            }

            sb.Append("]}\n");

            string dir = Path.Combine(jsonDir, configType.ToString(), relativeDir);
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
                Table table, ConfigType configType, StringBuilder sb)
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
                
                if (configType != ConfigType.cs && !prefix.Contains(configTypeStr))
                {
                    continue;
                }

                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }

                sb.Append($"[{worksheet.Cells[row, 3].Text.Trim()}, {{\"_t\":\"{name}\"");
                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
                {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!table.HeadInfos.ContainsKey(fieldName))
                    {
                        continue;
                    }

                    HeadInfo headInfo = table.HeadInfos[fieldName];

                    if (headInfo == null)
                    {
                        continue;
                    }

                    if (configType != ConfigType.cs && !headInfo.FieldCS.Contains(configTypeStr))
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

                sb.Append("}],\n");
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
                    if (string.IsNullOrEmpty(value))
                        return "[0]";

                    return $"[{value}]";
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
                    if (value == "")
                    {
                        return "0";
                    }

                    return value;
                case "string":
                    value = value.Replace("\\", "\\\\");
                    value = value.Replace("\"", "\\\"");
                    return $"\"{value}\"";
                case "bool":
                    {
                        if (value == "1")
                            return "true";
                        if (value == "0" || string.IsNullOrEmpty(value))
                            return "false";

                        return value;
                    }
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
        }

        #endregion


        // 根据生成的类，把json转成protobuf
        private static void ExportExcelProtobuf(ConfigType configType, Table table, string relativeDir)
        {
            string dir = GetProtoDir(configType, relativeDir);
            string moduleDir = Path.Combine(dir);
            if (!Directory.Exists(moduleDir))
            {
                Directory.CreateDirectory(moduleDir);
            }

            Assembly ass = GetAssembly(configType);
            Type type = ass.GetType($"ET.{table.Name}Category");

            IMerge final = Activator.CreateInstance(type) as IMerge;

            string p = Path.Combine(jsonDir, configType.ToString(), relativeDir);
            string[] ss = Directory.GetFiles(p, $"{table.Name}*.txt");
            List<string> jsonPaths = ss.ToList();

            jsonPaths.Sort();
            jsonPaths.Reverse();
            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                try
                {
                    object deserialize = BsonSerializer.Deserialize(json, type);
                    final.Merge(deserialize);
                }
                catch (Exception e)
                {
                    throw new Exception($"json : {jsonPath} error", e);
                }
            }

            string path = Path.Combine(moduleDir, $"{table.Name}Category.bytes");

            using FileStream file = File.Create(path);
            file.Write(final.ToBson());
        }
    }
}
