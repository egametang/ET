using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProtoBuf;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;
using System.IO;

public class Protogen {
    [MenuItem("Bundle/Generate Protocs")]
    public static void GenerateProtobufCS(){
		Generate(Application.dataPath + "/../Proto/",new string[] { "mmopb.proto" }, Application.dataPath + "/../HotFix/");
    }
    static void Generate(string inpath,string[] inprotos,string outpath)
    {

		var set = new FileDescriptorSet();

		set.AddImportPath(inpath);
		foreach (var inproto in inprotos) {
			set.Add (inproto, true);
		}

		set.Process();
		var errors = set.GetErrors();
		CSharpCodeGenerator.ClearTypeNames ();
		var files = CSharpCodeGenerator.Default.Generate(set);

		foreach (var file in files)
		{
			var path = Path.Combine(outpath, file.Name);
			File.WriteAllText(path, file.Text);

			Debug.Log($"generated: {path}");
		}
    }
}
