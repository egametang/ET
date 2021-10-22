using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace ET
{
	public interface IEntry
	{
		void Start();
		void Update();
		void LateUpdate();
		void OnApplicationQuit();
	}
	
	public class Init: MonoBehaviour
	{
		private IEntry entry;
		
		private void Awake()
		{
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			DontDestroyOnLoad(gameObject);

			Assembly modelAssembly = null;

			if (Define.IsEditor)
			{
				UnityEngine.Debug.Log("unity editor mode!");
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					string assemblyName = $"{assembly.GetName().Name}.dll";
					if (assemblyName != "Unity.ModelView.dll")
					{
						continue;
					}

					modelAssembly = assembly;
					break;
				}
			}
			else
			{
				byte[] log = new byte[1024];
				Interpreter.InterpreterSetLog((buff, n) =>
				{
					Marshal.Copy(buff, log, 0, n);
					UnityEngine.Debug.Log(log.Utf8ToStr(0, n));
				});
				Interpreter.InterpreterInit(@"E:\ET\Unity\UnityScript\", "Unity.Script.dll");
				
				/*
				UnityEngine.Debug.Log("unity script mode!");
				byte[] dllBytes = File.ReadAllBytes("./Temp/Bin/Debug/Unity.Script.dll");
				byte[] pdbBytes = File.ReadAllBytes("./Temp/Bin/Debug/Unity.Script.pdb");
				modelAssembly = Assembly.Load(dllBytes, pdbBytes);
				*/
			}

			Type initType = modelAssembly.GetType("ET.Entry");
			this.entry = Activator.CreateInstance(initType) as IEntry;
		}

		private void Start()
		{
			this.entry.Start();
		}

		private void Update()
		{
			this.entry.Update();
		}

		private void LateUpdate()
		{
			this.entry.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			this.entry.OnApplicationQuit();
		}
	}
}