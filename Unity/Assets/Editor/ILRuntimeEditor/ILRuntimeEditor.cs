using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Model;
using UnityEditor;

namespace MyEditor
{
	public class ILRuntimeEditor: EditorWindow
	{
		[MenuItem("ILRuntime/Generate CLR Binding Code")]
		static void GenerateCLRBinding()
		{
			List<Type> types = new List<Type>();
			//在List中添加你想进行CLR绑定的类型
			types.Add(typeof(int));
			types.Add(typeof(float));
			types.Add(typeof(long));
			types.Add(typeof(object));
			types.Add(typeof(string));
			types.Add(typeof(Console));
			types.Add(typeof(Array));
			types.Add(typeof(Dictionary<string, int>));
			types.Add(typeof(Type));
			//所有ILRuntime中的类型，实际上在C#运行时中都是ILRuntime.Runtime.Intepreter.ILTypeInstance的实例，
			//因此List<A> List<B>，如果A与B都是ILRuntime中的类型，只需要添加List<ILRuntime.Runtime.Intepreter.ILTypeInstance>即可
			types.Add(typeof(Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, int>));

			Assembly assemby = Game.EntityEventManager.GetAssembly("Model");
			foreach (Type type in assemby.GetTypes())
			{
				if (type.GetCustomAttributes(typeof (ILBindingAttribute), false).Length == 0)
				{
					continue;
				}
				Log.Info(type.FullName);
				types.Add(type);
			}

			//第二个参数为自动生成的代码保存在何处
			ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "Assets/Scripts/ILGenerated");
		}
	}
}
