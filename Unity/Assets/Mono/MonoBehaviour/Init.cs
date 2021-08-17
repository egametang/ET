﻿using System;
using System.Reflection;
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
			DontDestroyOnLoad(gameObject);
			
			Assembly modelAssembly = null;
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				string assemblyName = assembly.ManifestModule.Name;
				if (assemblyName != "Unity.ModelView.dll")
				{
					continue;
				}
				modelAssembly = assembly;
				break;
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