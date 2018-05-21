﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ETModel;
using UnityEditor;

namespace ETEditor
{
	class OpcodeInfo
	{
		public string Name;
		public int Opcode;
	}

	[Flags]
	public enum HeadFlag
	{
		None = 0,
		Bson = 1,
		Proto = 2,
	}
	
	public class Proto2CSEditor : EditorWindow
	{
		private const string protoPath = "../Proto/";
	    private const string protoHotFixPath = "../Proto/HotfixMessage/";
        private const string serverMessagePath = "../Server/Hotfix/Module/Message/";
		private const string clientMessagePath = "Assets/Scripts/Module/Message/";
		private const string hotfixMessagePath = "Hotfix/Module/Message/";
		private static readonly char[] splitChars = { ' ', '\t' };
		private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();
		private static MultiMap<string, string> parentMsg = new MultiMap<string, string>();
		
		[MenuItem("Tools/Proto2CS")]
		public static void AllProto2CS()
		{
			msgOpcode.Clear();
			Proto2CS("ETModel", "OuterMessage.proto", clientMessagePath, "OuterOpcode", 100, HeadFlag.Proto);
			GenerateOpcode("ETModel", "OuterOpcode", clientMessagePath);

			Proto2CS("ETHotfix", "OuterMessage.proto", serverMessagePath, "OuterOpcode", 100, HeadFlag.Proto | HeadFlag.Bson, false);
			GenerateOpcode("ETHotfix", "OuterOpcode", serverMessagePath);

			msgOpcode.Clear();
		    ProtoHotFixDir2CS("ETHotfix",  hotfixMessagePath, "HotfixOpcode", 10000, HeadFlag.None);
//			Proto2CS("ETHotfix", "HotfixMessage.proto", hotfixMessagePath, "HotfixOpcode", 10000, HeadFlag.None);
			GenerateOpcode("ETHotfix", "HotfixOpcode", hotfixMessagePath);

            msgOpcode.Clear();
		    ProtoHotFixDir2CS("ETHotfix",  serverMessagePath, "HotfixOpcode", 10000, HeadFlag.Bson, false);
//            Proto2CS("ETHotfix", "HotfixMessage.proto", serverMessagePath, "HotfixOpcode", 10000, HeadFlag.Bson, false);
			GenerateOpcode("ETHotfix", "HotfixOpcode", serverMessagePath);

			msgOpcode.Clear();
			Proto2CS("ETHotfix", "InnerMessage.proto", serverMessagePath, "InnerOpcode", 1000, HeadFlag.Bson, false);
			GenerateOpcode("ETHotfix", "InnerOpcode", serverMessagePath);

			AssetDatabase.Refresh();
		}

        public static void ProtoHotFixDir2CS(string ns,  string outputPath, string opcodeClassName, int startOpcode, HeadFlag flag, bool isClient = true)
        {
            msgOpcode.Clear();

            parentMsg = new MultiMap<string, string>();
            string csPath = Path.Combine(outputPath, "HotfixMessage" + ".cs");


            StringBuilder sb = new StringBuilder();
            sb.Append("using ProtoBuf;\n");
            sb.Append("using ETModel;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("using MongoDB.Bson.Serialization.Attributes;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            foreach (string filePath in Directory.GetFiles(protoHotFixPath))
            {
                if (Path.GetExtension(filePath) != ".proto")
                {
                    continue;
                }
                if (Path.GetFileName(filePath).StartsWith("~"))
                {
                    continue;
                }
                string protoName = Path.GetFileNameWithoutExtension(filePath);
                //开始读取一个
                string proto = Path.Combine(protoHotFixPath, protoName);

                sb.Append($"#region  {protoName}\n");

                string s = File.ReadAllText($"{proto}.proto");
                bool isMsgStart = false;
                string parentClass = "";
                foreach (string line in s.Split('\n'))
                {
                    string newline = line.Trim();

                    if (newline == "")
                    {
                        continue;
                    }

                    if (newline.StartsWith("//"))
                    {
                        sb.Append($"{newline}\n");
                    }

                    if (newline.StartsWith("message"))
                    {
                        parentClass = "";
                        isMsgStart = true;
                        string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                        string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                        if (ss.Length == 2)
                        {
                            parentClass = ss[1].Trim();
                        }

                        msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                        sb.Append($"\t[Message({opcodeClassName}.{msgName})]\n");
                        sb.Append($"\t[ProtoContract]\n");
                        sb.Append($"\tpublic partial class {msgName}");
                        if (parentClass == "IActorMessage" || parentClass == "IActorRequest" || parentClass == "IActorResponse" || parentClass == "IFrameMessage")
                        {
                            sb.Append($": {parentClass}\n");
                        }
                        else if (parentClass != "")
                        {
                            sb.Append($": {parentClass}\n");
                        }
                        else
                        {
                            sb.Append("\n");
                        }
                    }

                    if (isMsgStart && newline == "{")
                    {
                        sb.Append("\t{\n");

                        if (parentClass == "IRequest" || parentClass == "IActorRequest" || parentClass == "IActorMessage" || parentClass == "IFrameMessage")
                        {
                            sb.AppendLine("\t\t[ProtoMember(90, IsRequired = true)]");
                            sb.AppendLine("\t\tpublic int RpcId { get; set; }\n");
                        }

                        if (parentClass == "IResponse" || parentClass == "IActorResponse")
                        {
                            sb.AppendLine("\t\t[ProtoMember(90, IsRequired = true)]");
                            sb.AppendLine("\t\tpublic int RpcId { get; set; }\n");
                            sb.AppendLine("\t\t[ProtoMember(91, IsRequired = true)]");
                            sb.AppendLine("\t\tpublic int Error { get; set; }\n");
                            sb.AppendLine("\t\t[ProtoMember(92, IsRequired = true)]");
                            sb.AppendLine("\t\tpublic string Message { get; set; }\n");
                        }

                        if (parentClass == "IActorRequest" || parentClass == "IActorMessage")
                        {
                            sb.AppendLine("\t\t[ProtoMember(93, IsRequired = true)]");
                            sb.AppendLine("\t\tpublic long ActorId { get; set; }\n");
                        }

                        if (parentClass == "IFrameMessage")
                        {
                            sb.AppendLine("\t\t[ProtoMember(94, IsRequired = true)]");
                            sb.AppendLine("\t\tpublic long Id { get; set; }\n");
                        }
                    }

                    // 成员
                    if (newline.StartsWith("required"))
                    {
                        Members(sb, newline, true);
                    }

                    if (newline.StartsWith("optional"))
                    {
                        Members(sb, newline, false);
                    }

                    if (newline.StartsWith("repeated"))
                    {
                        Repeated(sb, ns, newline, isClient);
                    }

                    if (newline.StartsWith("object"))
                    {
                        Object(sb, newline);
                    }

                    if (newline.StartsWith("[") && newline.EndsWith("]"))
                    {
                        sb.AppendLine($"\t\t{newline}");
                    }

                    if (newline.StartsWith("#region") || newline.StartsWith("#endregion"))
                    {
                        sb.AppendLine($"\t{newline}");
                    }

                    if (isMsgStart && newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");
                    }
                }

                sb.Append($"#endregion\n");
            }
            



          
            sb.Append("}\n");

            //if (!isClient)
            //{
            //GenerateHead(sb, ns, flag, opcodeClassName);
            //}

            File.WriteAllText(csPath, sb.ToString());
        }
        public static void Proto2CS(string ns, string protoName, string outputPath, string opcodeClassName, int startOpcode, HeadFlag flag, bool isClient = true)
		{
			msgOpcode.Clear();
			parentMsg = new MultiMap<string, string>();
			string proto = Path.Combine(protoPath, protoName);
			string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");

			string s = File.ReadAllText(proto);

			StringBuilder sb = new StringBuilder();
			sb.Append("using ProtoBuf;\n");
			sb.Append("using ETModel;\n");
			sb.Append("using System.Collections.Generic;\n");
			sb.Append("using MongoDB.Bson.Serialization.Attributes;\n");
			sb.Append($"namespace {ns}\n");
			sb.Append("{\n");

			bool isMsgStart = false;
			string parentClass = "";
			foreach (string line in s.Split('\n'))
			{
				string newline = line.Trim();

				if (newline == "")
				{
					continue;
				}

				if (newline.StartsWith("//"))
				{
					sb.Append($"{newline}\n");
				}

				if (newline.StartsWith("message"))
				{
					parentClass = "";
					isMsgStart = true;
					string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
					string[] ss = newline.Split(new []{"//"}, StringSplitOptions.RemoveEmptyEntries);
					
					if (ss.Length == 2)
					{
						parentClass = ss[1].Trim();
					}

					msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode});
					
					sb.Append($"\t[Message({opcodeClassName}.{msgName})]\n");
					sb.Append($"\t[ProtoContract]\n");
					sb.Append($"\tpublic partial class {msgName}");
					if (parentClass == "IActorMessage" || parentClass == "IActorRequest" || parentClass == "IActorResponse" || parentClass == "IFrameMessage")
					{
						sb.Append($": {parentClass}\n");
					}
					else if (parentClass != "")
					{
						sb.Append($": {parentClass}\n");
					}
					else
					{
						sb.Append("\n");
					}
				}

				if (isMsgStart && newline == "{")
				{
					sb.Append("\t{\n");
					
					if (parentClass == "IRequest" || parentClass == "IActorRequest" || parentClass == "IActorMessage" || parentClass == "IFrameMessage")
					{
						sb.AppendLine("\t\t[ProtoMember(90, IsRequired = true)]");
						sb.AppendLine("\t\tpublic int RpcId { get; set; }\n");
					}

					if (parentClass == "IResponse" || parentClass == "IActorResponse")
					{
						sb.AppendLine("\t\t[ProtoMember(90, IsRequired = true)]");
						sb.AppendLine("\t\tpublic int RpcId { get; set; }\n");
						sb.AppendLine("\t\t[ProtoMember(91, IsRequired = true)]");
						sb.AppendLine("\t\tpublic int Error { get; set; }\n");
						sb.AppendLine("\t\t[ProtoMember(92, IsRequired = true)]");
						sb.AppendLine("\t\tpublic string Message { get; set; }\n");
					}

					if (parentClass == "IActorRequest" || parentClass == "IActorMessage")
					{
						sb.AppendLine("\t\t[ProtoMember(93, IsRequired = true)]");
						sb.AppendLine("\t\tpublic long ActorId { get; set; }\n");
					}

					if (parentClass == "IFrameMessage")
					{
						sb.AppendLine("\t\t[ProtoMember(94, IsRequired = true)]");
						sb.AppendLine("\t\tpublic long Id { get; set; }\n");
					}
				}

				// 成员
				if (newline.StartsWith("required"))
				{
					Members(sb, newline, true);
				}

				if (newline.StartsWith("optional"))
				{
					Members(sb, newline, false);
				}

				if (newline.StartsWith("repeated"))
				{
					Repeated(sb, ns, newline, isClient);
				}

			    if (newline.StartsWith("object"))
			    {
			        Object(sb, newline);
			    }

			    if (newline.StartsWith("[") && newline.EndsWith("]"))
			    {
			        sb.AppendLine($"\t\t{newline}");
			    }

			    if (newline.StartsWith("#region") || newline.StartsWith("#endregion"))
			    {
			        sb.AppendLine($"\t{newline}");
			    }

                if (isMsgStart && newline == "}")
				{
					isMsgStart = false;
					sb.Append("\t}\n\n");
				}
			}
			sb.Append("}\n");

			//if (!isClient)
			//{
				//GenerateHead(sb, ns, flag, opcodeClassName);
			//}

			File.WriteAllText(csPath, sb.ToString());
		}

		private static void GenerateHead(StringBuilder sb, string ns, HeadFlag flag, string opcodeClassName)
		{
			if ((flag & HeadFlag.Bson) != 0)
			{
				if (parentMsg.Count > 0)
				{
					sb.AppendLine($"namespace {ns}");
					sb.AppendLine("{");
					foreach (string parentClass in parentMsg.GetDictionary().Keys)
					{
						foreach (string s in parentMsg.GetAll(parentClass))
						{
							sb.Append($"\t[BsonKnownTypes(typeof({s}))]\n");
						}

						sb.Append($"\tpublic partial class {parentClass} {{}}\n\n");
					}

					sb.AppendLine("}");
				}
			}

			if ((flag & HeadFlag.Proto) != 0)
			{
				if (parentMsg.Count > 0)
				{
					sb.AppendLine($"namespace {ns}");
					sb.AppendLine("{");
					foreach (string parentClass in parentMsg.GetDictionary().Keys)
					{

						foreach (string s in parentMsg.GetAll(parentClass))
						{
							sb.Append($"\t[ProtoInclude({opcodeClassName}.{s}, typeof({s}))]\n");
						}

						sb.Append($"\tpublic partial class {parentClass} {{}}\n\n");
					}

					sb.AppendLine("}");
				}
			}
		}
		
		private static void GenerateOpcode(string ns, string outputFileName, string outputPath)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"namespace {ns}");
			sb.AppendLine("{");
			sb.AppendLine($"\tpublic static partial class {outputFileName}");
			sb.AppendLine("\t{");
			foreach (OpcodeInfo info in msgOpcode)
			{
				sb.AppendLine($"\t\t public const ushort {info.Name} = {info.Opcode};");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");
			
			string csPath = Path.Combine(outputPath, outputFileName + ".cs");
			File.WriteAllText(csPath, sb.ToString());
		}

		private static void Repeated(StringBuilder sb, string ns, string newline, bool isClient)
		{
			try
			{
				int index = newline.IndexOf(";");
				newline = newline.Remove(index);
				string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
				string type = ss[1];
				type = ConvertType(type);
				string name = ss[2];
				int order = int.Parse(ss[4]);
				if (isClient)
				{
					sb.Append($"\t\t[ProtoMember({order}, TypeName = \"{ns}.{type}\")]\n");
				}
				else
				{
					sb.Append($"\t\t[ProtoMember({order})]\n");
				}

				sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
			}
			catch (Exception e)
			{
				Log.Error($"{newline}\n {e}");
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
				string type = ss[1];
				string name = ss[2];
				int order = int.Parse(ss[4]);
				sb.Append($"\t\t[ProtoMember({order}, IsRequired = {isRequired.ToString().ToLower()})]\n");
				string typeCs = ConvertType(type);

				sb.Append($"\t\tpublic {typeCs} {name};\n\n");
			}
			catch (Exception e)
			{
				Log.Error($"{newline}\n {e}");
			}

		}

	    private static void Object(StringBuilder sb, string newline)
	    {
	        try
	        {
	            int index = newline.IndexOf(";");
	            newline = newline.Remove(index);
	            string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
	            string type = ss[1];
	            string name = ss[2];
	            int order = int.Parse(ss[4]);
	            sb.Append($"\t\t[ProtoMember({order})]\n");
	            sb.Append($"\t\tpublic {type} {name} = new {type}();\n\n");
	        }
	        catch (Exception e)
	        {
	            Log.Error($"{newline}\n {e}");
	        }

	    }
    }
}
