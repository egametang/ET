using System;
using System.IO;
using System.Reflection;
using Model;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		public static Init Instance;

		public ILRuntime.Runtime.Enviorment.AppDomain AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();

		private readonly object[] param0 = new object[0];

		private IMethod start;

		private IMethod update;

		private IMethod onApplicationQuit;

		private void Start()
		{
			try
			{
				Instance = this;

				AssemblyManager.Instance.Add("Model", typeof(Model.Init).Assembly);

				this.RegisterAssembly();
				this.RegisterILAdapter();
				this.RegisterDelegate();
				this.RegisterRedirection();

				IType hotfixInitType = AppDomain.LoadedTypes["Hotfix.Init"];
				start = hotfixInitType.GetMethod("Start", 0);
				update = hotfixInitType.GetMethod("Update", 0);
				onApplicationQuit = hotfixInitType.GetMethod("OnApplicationQuit", 0);

				// 进入热更新层
				this.AppDomain.Invoke(this.start, null, param0);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void Update()
		{
			this.AppDomain.Invoke(this.update, null, this.param0);

			ObjectEvents.Instance.Update();
		}

		private void OnApplicationQuit()
		{
			this.AppDomain.Invoke(this.onApplicationQuit, null, this.param0);
		}

		public void RegisterAssembly()
		{
			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.GetComponent<ReferenceCollector>().Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.GetComponent<ReferenceCollector>().Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}
		}

		public unsafe void RegisterRedirection()
		{
			MethodInfo mi = typeof(Log).GetMethod("Debug", new Type[] { typeof(string) });
			this.AppDomain.RegisterCLRMethodRedirection(mi, ILRedirection.LogDebug);
		}

		public void RegisterDelegate()
		{
			AppDomain.DelegateManager.RegisterMethodDelegate<AChannel, System.Net.Sockets.SocketError>();
			AppDomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();

		}

		public void RegisterILAdapter()
		{
			Assembly assembly = typeof(Init).Assembly;

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
				AppDomain.RegisterCrossBindingAdaptor(adaptor);
			}
		}
	}
}