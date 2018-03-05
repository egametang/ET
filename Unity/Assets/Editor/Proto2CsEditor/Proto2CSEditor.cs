using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ETModel;
using UnityEditor;

namespace MyEditor
{
	class OpcodeInfo
	{
		public string Name;
		public int Opcode;
	}

	[Flags]
	public enum HeadFlag
	{
		Bson = 1,
		Proto = 2,
	}
	
	public class Proto2CSEditor : EditorWindow
	{
		private const string protoPath = @"..\Proto\";
		private const string innerOutPath = @"..\Server\Model\Module\Message\";
		private const string outerOutPath = @"Assets\Scripts\Module\Message\";
		private const string hotfixOutPath = @"Hotfix\Module\Message\";
		private static readonly char[] splitChars = { ' ', '\t' };
		private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();
		private static MultiMap<string, string> parentMsg = new MultiMap<string, string>();
		
		[MenuItem("Tools/Proto2CS")]
		public static void AllProto2CS()
		{
			msgOpcode.Clear();
			Proto2CS("ETModel", "OuterMessage.proto", outerOutPath, "OuterOpcode", 100, HeadFlag.Proto | HeadFlag.Bson);
			GenerateOpcode("OuterOpcode", outerOutPath);

			msgOpcode.Clear();
			Proto2CS("ETModel", "InnerMessage.proto", innerOutPath, "InnerOpcode", 1000, HeadFlag.Bson);
			GenerateOpcode("InnerOpcode", innerOutPath);

			msgOpcode.Clear();
			Proto2CS("Hotfix", "HotfixMessage.proto", hotfixOutPath, "HotfixOpcode", 10000, HeadFlag.Bson);
			GenerateOpcode("HotfixOpcode", hotfixOutPath);
			
			AssetDatabase.Refresh();
		}
		
		public static void Proto2CS(string ns, string protoName, string outputPath, string opcodeClassName, int startOpcode, HeadFlag flag)
		{
			msgOpcode.Clear();
			parentMsg = new MultiMap<string, string>();
			string proto = Path.Combine(protoPath, protoName);
			string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");

			string s = File.ReadAllText(proto);

			StringBuilder sb = new StringBuilder();
			sb.Append("using ProtoBuf;\n");
			sb.Append("using ETModel;\n");
			if (ns == "Hotfix")
			{
				sb.Append("using Hotfix;\n");
			}

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
						sb.Append($": MessageObject, {parentClass}\n");
						parentMsg.Add("MessageObject", msgName);
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

					if (parentClass == "IResponse" || parentClass == "IActorResponse")
					{
						sb.AppendLine("\t\t[ProtoMember(90, IsRequired = true)]");
						sb.AppendLine("\t\tpublic int Error { get; set; }");
						sb.AppendLine("\t\t[ProtoMember(91, IsRequired = true)]");
						sb.AppendLine("\t\tpublic string Message { get; set; }");
					}

					if (parentClass == "IFrameMessage")
					{
						sb.AppendLine("\t\t[ProtoMember(92, IsRequired = true)]");
						sb.AppendLine("\t\tpublic long Id { get; set; }");
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
					Repeated(sb, newline);
				}

				if (isMsgStart && newline == "}")
				{
					isMsgStart = false;
					sb.Append("\t}\n\n");
				}
			}
			sb.Append("}\n");

			GenerateHead(sb, flag, opcodeClassName);

			File.WriteAllText(csPath, sb.ToString());
		}

		private static void GenerateHead(StringBuilder sb, HeadFlag flag, string opcodeClassName)
		{
			sb.AppendLine("#if SERVER");
			sb.AppendLine("namespace ETModel\n{");
			foreach (string parentClass in parentMsg.GetDictionary().Keys)
			{
				if ((flag & HeadFlag.Bson) != 0)
				{
					foreach (string s in parentMsg.GetAll(parentClass))
					{
						sb.Append($"\t[BsonKnownTypes(typeof({s}))]\n");
					}
				}


				sb.Append($"\tpublic partial class {parentClass} {{}}\n\n");
			}
			sb.AppendLine("}");
			sb.AppendLine("#endif");

			sb.AppendLine("namespace ETModel\n{");
			foreach (string parentClass in parentMsg.GetDictionary().Keys)
			{
				if ((flag & HeadFlag.Proto) != 0)
				{
					foreach (string s in parentMsg.GetAll(parentClass))
					{
						sb.Append($"\t[ProtoInclude({opcodeClassName}.{s}, typeof({s}))]\n");
					}
				}

				sb.Append($"\tpublic partial class {parentClass} {{}}\n\n");
			}
			sb.AppendLine("}");
		}
		
		private static void GenerateOpcode(string outputFileName, string outputPath)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("namespace ETModel");
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

		private static void Repeated(StringBuilder sb, string newline)
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
				sb.Append($"\t\t[ProtoMember({order})]\n");
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
	}
}
