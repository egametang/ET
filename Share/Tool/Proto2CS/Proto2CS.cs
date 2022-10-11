using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ET
{
    internal class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    public static class Proto2CS
    {
        public static void Export()
        {
            // InnerMessage.proto生成cs代码
            InnerProto2CS.Proto2CS();
            Log.Console("proto2cs succeed!");
        }
    }

    public static class InnerProto2CS
    {
        private const string protoDir = "../Unity/Assets/Config/Proto";
        private const string clientMessagePath = "../Unity/Assets/Scripts/Codes/Model/Generate/Client/Message/";
        private const string serverMessagePath = "../Unity/Assets/Scripts/Codes/Model/Generate/Server/Message/";
        private const string clientServerMessagePath = "../Unity/Assets/Scripts/Codes/Model/Generate/ClientServer/Message/";
        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2CS()
        {
            msgOpcode.Clear();

            if (Directory.Exists(clientMessagePath))
            {
                Directory.Delete(clientMessagePath, true);
            }

            if (Directory.Exists(serverMessagePath))
            {
                Directory.Delete(serverMessagePath, true);
            }
            
            if (Directory.Exists(clientServerMessagePath))
            {
                Directory.Delete(clientServerMessagePath, true);
            }

            List<string> list = FileHelper.GetAllFiles(protoDir, "*proto");
            foreach (string s in list)
            {
                if (!s.EndsWith(".proto"))
                {
                    continue;
                }
                string fileName = Path.GetFileNameWithoutExtension(s);
                string[] ss2 = fileName.Split('_');
                string protoName = ss2[0];
                string cs = ss2[1];
                int startOpcode = int.Parse(ss2[2]);
                ProtoFile2CS(fileName, protoName, cs, startOpcode);
            }
        }

        public static void ProtoFile2CS(string fileName, string protoName, string cs, int startOpcode)
        {
            string ns = "ET";
            msgOpcode.Clear();
            string proto = Path.Combine(protoDir, $"{fileName}.proto");
            
            string s = File.ReadAllText(proto);

            StringBuilder sb = new StringBuilder();
            sb.Append("using ET;\n");
            sb.Append("using ProtoBuf;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");
            
            bool isMsgStart = false;
            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//ResponseType"))
                {
                    string responseType = line.Split(" ")[1].TrimEnd('\r', '\n');
                    sb.Append($"\t[ResponseType(nameof({responseType}))]\n");
                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"{newline}\n");
                    continue;
                }

                if (newline.StartsWith("message"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                    sb.Append($"\t[Message({protoName}.{msgName})]\n");
                    sb.Append($"\t[ProtoContract]\n");
                    sb.Append($"\tpublic partial class {msgName}: ProtoObject");
                    if (parentClass == "IActorMessage" || parentClass == "IActorRequest" || parentClass == "IActorResponse")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else if (parentClass != "")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    continue;
                }

                if (isMsgStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");
                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.Append($"{newline}\n");
                        continue;
                    }

                    if (newline.Trim() != "" && newline != "}")
                    {
                        if (newline.StartsWith("map<"))
                        {
                            Map(sb, ns, newline);
                        }
                        else if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, ns, newline);
                        }
                        else
                        {
                            Members(sb, newline, true);
                        }
                    }
                }
            }


            sb.Append("\tpublic static class " + protoName + "\n\t{\n");
            foreach (OpcodeInfo info in msgOpcode)
            {
                sb.Append($"\t\t public const ushort {info.Name} = {info.Opcode};\n");
            }

            sb.Append("\t}\n");
            

            sb.Append("}\n");

            if (cs.Contains("C"))
            {
                GenerateCS(sb, clientMessagePath, proto);
                GenerateCS(sb, serverMessagePath, proto);
                GenerateCS(sb, clientServerMessagePath, proto);
            }
            
            if (cs.Contains("S"))
            {
                GenerateCS(sb, serverMessagePath, proto);
                GenerateCS(sb, clientServerMessagePath, proto);
            }
        }

        private static void GenerateCS(StringBuilder sb, string path, string proto)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string csPath = Path.Combine(path, Path.GetFileNameWithoutExtension(proto) + ".cs");
            using FileStream txt = new FileStream(csPath, FileMode.Create, FileAccess.ReadWrite);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }

        private static void Map(StringBuilder sb, string ns, string newline)
        {
            int start = newline.IndexOf("<") + 1;
            int end = newline.IndexOf(">");
            string types = newline.Substring(start, end - start);
            string[] ss = types.Split(",");
            string keyType = ConvertType(ss[0].Trim());
            string valueType = ConvertType(ss[1].Trim());
            string tail = newline.Substring(end + 1);
            ss = tail.Trim().Replace(";", "").Split(" ");
            string v = ss[0];
            string n = ss[2];
            
            sb.Append("\t\t[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]\n");
            sb.Append($"\t\t[ProtoMember({n})]\n");
            sb.Append($"\t\tpublic Dictionary<{keyType}, {valueType}> {v} {{ get; set; }}\n");
        }
        
        private static void Repeated(StringBuilder sb, string ns, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int n = int.Parse(ss[4]);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic List<{type}> {name} {{ get; set; }}\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }

        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[0];
                string name = ss[1];
                int n = int.Parse(ss[3]);
                string typeCs = ConvertType(type);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }
    }
}