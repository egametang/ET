using System;
using System.IO;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;

namespace Model
{
	[EntityEvent(typeof(ILRuntimeComponent))]
	public class ILRuntimeComponent : Component
	{
		private ILRuntime.Runtime.Enviorment.AppDomain appDomain;

		private readonly object[] param0 = new object[0];

		private IMethod start;

		private void Awake()
		{
			appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();

			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				appDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}

			Assembly assembly = Game.EntityEventManager.GetAssembly("Model");

			foreach (Type type in assembly.GetTypes())
			{
				object[] attrs = type.GetCustomAttributes(typeof(ILAdapterAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(type);
				CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
				if (adaptor == null)
				{
					continue;
				}
				appDomain.RegisterCrossBindingAdaptor(adaptor);
			}
			
			this.start = appDomain.LoadedTypes["Hotfix.HotfixEntry"].GetMethod("Start", 0);
			
			appDomain.Invoke(this.start, null, param0);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}