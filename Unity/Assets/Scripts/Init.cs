using System;
using System.IO;
using Base;
using ILRuntime.CLR.Method;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private ILRuntime.Runtime.Enviorment.AppDomain appDomain;

		private readonly object[] param0 = new object[0];

		private IMethod start;

		private IMethod update;

		private void Start()
		{
			appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();

			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);

			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				appDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}

			appDomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineClassInheritanceAdaptor());

			this.start = appDomain.LoadedTypes["Hotfix.Init"].GetMethod("Start", 0);
			this.update = appDomain.LoadedTypes["Hotfix.Init"].GetMethod("Update", 0);


			appDomain.Invoke(this.start, null, param0);
		}

		private void Update()
		{
			try
			{
				appDomain.Invoke(this.update, null, param0);
				Game.EntityEventManager.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void OnApplicationQuit()
		{
			Game.CloseScene();
		}
	}
}