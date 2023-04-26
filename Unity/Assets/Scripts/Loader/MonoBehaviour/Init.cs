using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
using MemoryPack;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};
				
			Game.AddSingleton<MainThreadSynchronizationContext>();

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed(Game.AddSingleton);
			Options.Instance.StartConfig = "StartConfig/LockStep";
			
			Game.AddSingleton<TimeInfo>();
			Game.AddSingleton<Logger>().ILog = new UnityLogger();
			Game.AddSingleton<ObjectPool>();
			Game.AddSingleton<IdGenerater>();
			Game.AddSingleton<EventSystem>();
			Game.AddSingleton<TimerComponent>();
			Game.AddSingleton<CoroutineLockComponent>();

			ETTask.ExceptionHandler += Log.Error;

			Game.AddSingleton<CodeLoader>().Start();
			
			this.cc = new Cc() { aaa = new Aa()};
			cc.bbb = new List<Bb>();
			cc.bbb.Add(new Bb() {B = 5});
			cc.ccc = new Dictionary<long, Bb>();
			cc.ccc.Add(1, new Bb() {B = 4});
			fixedArrayBufferWriter = new FixedArrayBufferWriter(this.bytes);
			//Dd d = new Dd() { aaa = 1, bbb = "ggs" };
			//d.ccc = new List<int>() { 1, 2, 3 };
			//bytes = MemoryPackSerializer.Serialize(d);
		}

		private byte[] bytes = new byte[1024];
		private Cc cc;
		private Cc cs = new Cc() {bbb = new List<Bb>(), ccc = new Dictionary<long, Bb>()};
		private Dd dd;

		private FixedArrayBufferWriter fixedArrayBufferWriter;

		private void Update()
		{
			fixedArrayBufferWriter.Reset(this.bytes);
			MemoryPackSerializer.Serialize(typeof(Cc), fixedArrayBufferWriter, this.cc);
			MemoryPackSerializer.Deserialize(bytes, ref this.cs);
			//this.bytes = MemoryPackSerializer.Serialize(this.cc);
			//MemoryPackSerializer.Deserialize(bytes, ref this.cs);
			Game.Update();
		}

		private void LateUpdate()
		{
			Game.LateUpdate();
			Game.FrameFinishUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Close();
		}
	}
	
	
}