public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// CommandLine.dll
	// MemoryPack.dll
	// MongoDB.Driver.Core.dll
	// MongoDB.Driver.dll
	// System.Core.dll
	// System.Runtime.CompilerServices.Unsafe.dll
	// System.dll
	// Unity.Codes.dll
	// Unity.Core.dll
	// Unity.Loader.dll
	// Unity.ThirdParty.dll
	// UnityEngine.CoreModule.dll
	// mscorlib.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// ET.AEvent<object,ET.Server.EventType.UnitEnterSightRange>
	// ET.AEvent<object,ET.EventType.AfterCreateClientScene>
	// ET.AEvent<object,ET.EventType.AfterCreateCurrentScene>
	// ET.AEvent<object,ET.EventType.SceneChangeStart>
	// ET.AEvent<object,ET.Client.NetClientComponentOnRead>
	// ET.AEvent<object,ET.EventType.SceneChangeFinish>
	// ET.AEvent<object,ET.EventType.LoginFinish>
	// ET.AEvent<object,ET.EventType.AppStartInitFinish>
	// ET.AEvent<object,ET.EventType.NumbericChange>
	// ET.AEvent<object,ET.EventType.EntryEvent2>
	// ET.AEvent<object,ET.EventType.AfterUnitCreate>
	// ET.AEvent<object,ET.EventType.EntryEvent1>
	// ET.AEvent<object,ET.EventType.ChangeRotation>
	// ET.AEvent<object,ET.Server.NetServerComponentOnRead>
	// ET.AEvent<object,ET.EventType.LSAfterUnitCreate>
	// ET.AEvent<object,ET.EventType.LSSceneChangeStart>
	// ET.AEvent<object,ET.EventType.LSSceneInitFinish>
	// ET.AEvent<object,ET.Server.NetInnerComponentOnRead>
	// ET.AEvent<object,ET.Server.EventType.UnitLeaveSightRange>
	// ET.AEvent<object,ET.EventType.ChangePosition>
	// ET.AEvent<object,ET.EventType.EntryEvent3>
	// ET.AInvokeHandler<ET.ConfigComponent.GetOneConfigBytes,object>
	// ET.AInvokeHandler<ET.ConfigComponent.GetAllConfigBytes,object>
	// ET.AInvokeHandler<ET.Server.RobotInvokeArgs,object>
	// ET.AInvokeHandler<ET.NavmeshComponent.RecastFileLoader,object>
	// ET.AMHandler<object>
	// ET.ATimer<object>
	// ET.AwakeSystem<object>
	// ET.AwakeSystem<object,object>
	// ET.AwakeSystem<object,int>
	// ET.AwakeSystem<object,System.Net.Sockets.AddressFamily>
	// ET.AwakeSystem<object,ET.Server.MailboxType>
	// ET.AwakeSystem<object,object,int>
	// ET.AwakeSystem<object,long,object>
	// ET.AwakeSystem<object,object,object>
	// ET.AwakeSystem<object,int,Unity.Mathematics.float3>
	// ET.AwakeSystem<object,object,object,int>
	// ET.ConfigSingleton<object>
	// ET.DestroySystem<object>
	// ET.EntityRef<object>
	// ET.ETAsyncTaskMethodBuilder<int>
	// ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>
	// ET.ETAsyncTaskMethodBuilder<byte>
	// ET.ETAsyncTaskMethodBuilder<uint>
	// ET.ETAsyncTaskMethodBuilder<object>
	// ET.ETAsyncTaskMethodBuilder<long>
	// ET.ETTask<ET.WaitType.Wait_Room2C_Start>
	// ET.ETTask<ET.Client.Wait_CreateMyUnit>
	// ET.ETTask<object>
	// ET.ETTask<int>
	// ET.ETTask<long>
	// ET.ETTask<System.ValueTuple<uint,object>>
	// ET.ETTask<ET.Client.Wait_UnitStop>
	// ET.ETTask<ET.Client.Wait_SceneChangeFinish>
	// ET.ETTask<byte>
	// ET.ETTask<uint>
	// ET.ETTask<ET.RobotCase_SecondCaseWait>
	// ET.IAwake<ET.Server.MailboxType>
	// ET.IAwake<int>
	// ET.IAwake<object>
	// ET.IAwake<System.Net.Sockets.AddressFamily>
	// ET.IAwake<object,int>
	// ET.IAwake<long,object>
	// ET.IAwake<object,object>
	// ET.IAwake<int,Unity.Mathematics.float3>
	// ET.IAwake<object,object,int>
	// ET.LateUpdateSystem<object>
	// ET.ListComponent<object>
	// ET.ListComponent<long>
	// ET.ListComponent<Unity.Mathematics.float3>
	// ET.LoadSystem<object>
	// ET.LSUpdateSystem<object>
	// ET.MultiMap<int,object>
	// ET.RollbackSystem<object>
	// ET.Server.AMActorHandler<object,object>
	// ET.Server.AMActorLocationHandler<object,object>
	// ET.Server.AMActorLocationRpcHandler<object,object,object>
	// ET.Server.AMActorRpcHandler<object,object,object>
	// ET.Server.AMRpcHandler<object,object>
	// ET.Singleton<object>
	// ET.UnOrderMultiMap<object,object>
	// ET.UpdateSystem<object>
	// MemoryPack.Formatters.ArrayFormatter<ET.LSInput>
	// MemoryPack.Formatters.ArrayFormatter<object>
	// MemoryPack.Formatters.ArrayFormatter<byte>
	// MemoryPack.Formatters.DictionaryFormatter<long,ET.LSInput>
	// MemoryPack.Formatters.DictionaryFormatter<int,long>
	// MemoryPack.Formatters.ListFormatter<long>
	// MemoryPack.Formatters.ListFormatter<object>
	// MemoryPack.Formatters.ListFormatter<Unity.Mathematics.float3>
	// MemoryPack.IMemoryPackable<object>
	// MemoryPack.IMemoryPackable<ET.LSInput>
	// MemoryPack.MemoryPackFormatter<object>
	// MemoryPack.MemoryPackFormatter<ET.LSInput>
	// MongoDB.Driver.IMongoCollection<object>
	// System.Action<object>
	// System.Action<long,object>
	// System.Action<long,int>
	// System.Action<long,long,object>
	// System.Collections.Generic.Dictionary<int,long>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<ushort,object>
	// System.Collections.Generic.Dictionary<long,ET.EntityRef<object>>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,long>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,long>
	// System.Collections.Generic.Dictionary<long,ET.LSInput>
	// System.Collections.Generic.Dictionary<uint,object>
	// System.Collections.Generic.Dictionary<int,ET.RpcInfo>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,ET.LSInput>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.HashSet<long>
	// System.Collections.Generic.HashSet<ushort>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet.Enumerator<long>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.KeyValuePair<long,ET.LSInput>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<uint,object>
	// System.Collections.Generic.KeyValuePair<int,ET.Server.ActorMessageSender>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<int,long>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<Unity.Mathematics.float3>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<Unity.Mathematics.float3>
	// System.Collections.Generic.SortedDictionary<long,object>
	// System.Collections.Generic.SortedDictionary<object,object>
	// System.Collections.Generic.SortedDictionary<int,object>
	// System.Collections.Generic.SortedDictionary<int,ET.Server.ActorMessageSender>
	// System.Collections.Generic.SortedDictionary.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<int,ET.Server.ActorMessageSender>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<int,object>
	// System.Func<object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,int>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<object,int>,object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<uint,uint>>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.Task<System.ValueTuple<uint,uint>>
	// System.ValueTuple<uint,uint>
	// System.ValueTuple<uint,object>
	// }}

	public void RefMethods()
	{
		// CommandLine.ParserResult<object> CommandLine.Parser.ParseArguments<object>(System.Collections.Generic.IEnumerable<string>)
		// CommandLine.ParserResult<object> CommandLine.ParserResultExtensions.WithNotParsed<object>(CommandLine.ParserResult<object>,System.Action<System.Collections.Generic.IEnumerable<CommandLine.Error>>)
		// CommandLine.ParserResult<object> CommandLine.ParserResultExtensions.WithParsed<object>(CommandLine.ParserResult<object>,System.Action<object>)
		// object ET.Client.GameObjectHelper.Get<object>(UnityEngine.GameObject,string)
		// object ET.Entity.AddChild<object,long,object>(long,object,bool)
		// object ET.Entity.AddChild<object>(bool)
		// object ET.Entity.AddChild<object,object,object,int>(object,object,int,bool)
		// object ET.Entity.AddChild<object,int>(int,bool)
		// object ET.Entity.AddChild<object,object,object>(object,object,bool)
		// object ET.Entity.AddChild<object,object>(object,bool)
		// object ET.Entity.AddChildWithId<object>(long,bool)
		// object ET.Entity.AddChildWithId<object,int>(long,int,bool)
		// object ET.Entity.AddChildWithId<object,object>(long,object,bool)
		// object ET.Entity.AddComponent<object,int,Unity.Mathematics.float3>(int,Unity.Mathematics.float3,bool)
		// object ET.Entity.AddComponent<object>(bool)
		// object ET.Entity.AddComponent<object,object>(object,bool)
		// object ET.Entity.AddComponent<object,ET.Server.MailboxType>(ET.Server.MailboxType,bool)
		// object ET.Entity.AddComponent<object,object,object>(object,object,bool)
		// object ET.Entity.AddComponent<object,object,int>(object,int,bool)
		// object ET.Entity.AddComponent<object,int>(int,bool)
		// object ET.Entity.AddComponent<object,System.Net.Sockets.AddressFamily>(System.Net.Sockets.AddressFamily,bool)
		// object ET.Entity.AddComponentWithId<object>(long,bool)
		// object ET.Entity.GetChild<object>(long)
		// object ET.Entity.GetComponent<object>()
		// object ET.Entity.GetParent<object>()
		// System.Void ET.Entity.RemoveComponent<object>()
		// ET.SceneType ET.EnumHelper.FromString<ET.SceneType>(string)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<uint,uint>>,object>(System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<uint,uint>>&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,object>(ET.ETTaskCompleted&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,object>(ET.ETTaskCompleted&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<uint>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<int>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<uint>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<byte>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<int>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<object>(object&)
		// object ET.EventSystem.Invoke<ET.NavmeshComponent.RecastFileLoader,object>(ET.NavmeshComponent.RecastFileLoader)
		// object ET.EventSystem.Invoke<ET.Server.RobotInvokeArgs,object>(int,ET.Server.RobotInvokeArgs)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.AfterCreateClientScene>(object,ET.EventType.AfterCreateClientScene)
		// System.Void ET.EventSystem.Publish<object,ET.Client.NetClientComponentOnRead>(object,ET.Client.NetClientComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.Server.EventType.UnitEnterSightRange>(object,ET.Server.EventType.UnitEnterSightRange)
		// System.Void ET.EventSystem.Publish<object,ET.Server.EventType.UnitLeaveSightRange>(object,ET.Server.EventType.UnitLeaveSightRange)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.AfterUnitCreate>(object,ET.EventType.AfterUnitCreate)
		// System.Void ET.EventSystem.Publish<object,ET.Server.NetInnerComponentOnRead>(object,ET.Server.NetInnerComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.Server.NetServerComponentOnRead>(object,ET.Server.NetServerComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.NumbericChange>(object,ET.EventType.NumbericChange)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.ChangePosition>(object,ET.EventType.ChangePosition)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.LSSceneInitFinish>(object,ET.EventType.LSSceneInitFinish)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.AfterCreateCurrentScene>(object,ET.EventType.AfterCreateCurrentScene)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.SceneChangeFinish>(object,ET.EventType.SceneChangeFinish)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.MoveStart>(object,ET.EventType.MoveStart)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.LSAfterUnitCreate>(object,ET.EventType.LSAfterUnitCreate)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.MoveStop>(object,ET.EventType.MoveStop)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.EnterMapFinish>(object,ET.EventType.EnterMapFinish)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.SceneChangeStart>(object,ET.EventType.SceneChangeStart)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.ChangeRotation>(object,ET.EventType.ChangeRotation)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.LSSceneChangeStart>(object,ET.EventType.LSSceneChangeStart)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.EntryEvent2>(object,ET.EventType.EntryEvent2)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.EntryEvent1>(object,ET.EventType.EntryEvent1)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.AppStartInitFinish>(object,ET.EventType.AppStartInitFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.LoginFinish>(object,ET.EventType.LoginFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.EntryEvent3>(object,ET.EventType.EntryEvent3)
		// object ET.Game.AddSingleton<object>()
		// object ET.JsonHelper.FromJson<object>(string)
		// object ET.LSEntity.AddComponent<object>(bool)
		// object ET.MongoHelper.Deserialize<object>(byte[])
		// System.Void ET.MongoHelper.RegisterStruct<ET.LSInput>()
		// object ET.NetServices.FetchMessage<object>()
		// System.Void ET.ObjectHelper.Swap<object>(object&,object&)
		// System.Void ET.ObjectWaitSystem.Notify<ET.WaitType.Wait_Room2C_Start>(ET.ObjectWait,ET.WaitType.Wait_Room2C_Start)
		// System.Void ET.ObjectWaitSystem.Notify<ET.RobotCase_SecondCaseWait>(ET.ObjectWait,ET.RobotCase_SecondCaseWait)
		// System.Void ET.ObjectWaitSystem.Notify<ET.Client.Wait_UnitStop>(ET.ObjectWait,ET.Client.Wait_UnitStop)
		// System.Void ET.ObjectWaitSystem.Notify<ET.Client.Wait_CreateMyUnit>(ET.ObjectWait,ET.Client.Wait_CreateMyUnit)
		// System.Void ET.ObjectWaitSystem.Notify<ET.Client.Wait_SceneChangeFinish>(ET.ObjectWait,ET.Client.Wait_SceneChangeFinish)
		// ET.ETTask<ET.WaitType.Wait_Room2C_Start> ET.ObjectWaitSystem.Wait<ET.WaitType.Wait_Room2C_Start>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.Client.Wait_UnitStop> ET.ObjectWaitSystem.Wait<ET.Client.Wait_UnitStop>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.Client.Wait_CreateMyUnit> ET.ObjectWaitSystem.Wait<ET.Client.Wait_CreateMyUnit>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.Client.Wait_SceneChangeFinish> ET.ObjectWaitSystem.Wait<ET.Client.Wait_SceneChangeFinish>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.RobotCase_SecondCaseWait> ET.ObjectWaitSystem.Wait<ET.RobotCase_SecondCaseWait>(ET.ObjectWait,ET.ETCancellationToken)
		// System.Void ET.RandomGenerator.BreakRank<object>(System.Collections.Generic.List<object>)
		// object ET.RandomGenerator.RandomArray<object>(System.Collections.Generic.List<object>)
		// string ET.StringHelper.ArrayToString<float>(float[])
		// System.Void MemoryPack.Formatters.ListFormatter.DeserializePackable<object>(MemoryPack.MemoryPackReader&,System.Collections.Generic.List<object>&)
		// System.Collections.Generic.List<object> MemoryPack.Formatters.ListFormatter.DeserializePackable<object>(MemoryPack.MemoryPackReader&)
		// System.Void MemoryPack.Formatters.ListFormatter.SerializePackable<object>(MemoryPack.MemoryPackWriter&,System.Collections.Generic.List<object>&)
		// bool MemoryPack.MemoryPackFormatterProvider.IsRegistered<object>()
		// bool MemoryPack.MemoryPackFormatterProvider.IsRegistered<ET.LSInput>()
		// System.Void MemoryPack.MemoryPackFormatterProvider.Register<ET.LSInput>(MemoryPack.MemoryPackFormatter<ET.LSInput>)
		// System.Void MemoryPack.MemoryPackFormatterProvider.Register<object>(MemoryPack.MemoryPackFormatter<object>)
		// System.Void MemoryPack.MemoryPackReader.ReadPackable<object>(object&)
		// object MemoryPack.MemoryPackReader.ReadPackable<object>()
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<TrueSync.TSVector>(TrueSync.TSVector&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,TrueSync.TSVector,TrueSync.TSQuaternion>(byte&,long&,TrueSync.TSVector&,TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long>(byte&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int>(byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int,long>(int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long,long,long>(byte&,int&,int&,long&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<TrueSync.TSQuaternion>(TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long,long,int>(byte&,int&,int&,long&,long&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long,long>(byte&,int&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int,int>(int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long>(byte&,int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long>(byte&,int&,int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int>(byte&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,long>(byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,Unity.Mathematics.float3>(byte&,int&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.float3>(Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,int,int,Unity.Mathematics.float3,Unity.Mathematics.float3>(byte&,long&,int&,int&,Unity.Mathematics.float3&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,ET.LSInput>(byte&,int&,long&,ET.LSInput&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.quaternion>(Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte>(byte&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<ET.LSInput>(ET.LSInput&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,Unity.Mathematics.float3>(byte&,long&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.quaternion,int>(Unity.Mathematics.quaternion&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,Unity.Mathematics.float3,Unity.Mathematics.quaternion>(byte&,int&,long&,Unity.Mathematics.float3&,Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<uint>(uint&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,uint>(byte&,uint&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanagedArray<byte>(byte[]&)
		// byte[] MemoryPack.MemoryPackReader.ReadUnmanagedArray<byte>()
		// object MemoryPack.MemoryPackReader.ReadValue<object>()
		// System.Void MemoryPack.MemoryPackReader.ReadValue<object>(object&)
		// System.Void MemoryPack.MemoryPackWriter.WritePackable<object>(object&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<ET.LSInput>(ET.LSInput&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<int,long>(int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<Unity.Mathematics.quaternion,int>(Unity.Mathematics.quaternion&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedArray<byte>(byte[])
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte>(byte,byte&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,long>(byte,byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<int,int>(byte,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long>(byte,byte&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long,long,long>(byte,byte&,int&,int&,long&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int>(byte,byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,Unity.Mathematics.float3,Unity.Mathematics.quaternion>(byte,byte&,int&,long&,Unity.Mathematics.float3&,Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long>(byte,byte&,int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long,long>(byte,byte&,int&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long,long,int>(byte,byte&,int&,int&,long&,long&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long>(byte,byte&,int&,int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,Unity.Mathematics.float3>(byte,byte&,long&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,TrueSync.TSVector,TrueSync.TSQuaternion>(byte,byte&,long&,TrueSync.TSVector&,TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,Unity.Mathematics.float3>(byte,byte&,int&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,int,int,Unity.Mathematics.float3,Unity.Mathematics.float3>(byte,byte&,long&,int&,int&,Unity.Mathematics.float3&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,ET.LSInput>(byte,byte&,int&,long&,ET.LSInput&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int>(byte,byte&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,uint>(byte,byte&,uint&)
		// System.Void MemoryPack.MemoryPackWriter.WriteValue<object>(object&)
		// System.Threading.Tasks.Task<object> MongoDB.Driver.IAsyncCursorExtensions.FirstOrDefaultAsync<object>(MongoDB.Driver.IAsyncCursor<object>,System.Threading.CancellationToken)
		// System.Threading.Tasks.Task<MongoDB.Driver.IAsyncCursor<object>> MongoDB.Driver.IMongoCollectionExtensions.FindAsync<object>(MongoDB.Driver.IMongoCollection<object>,System.Linq.Expressions.Expression<System.Func<object,bool>>,MongoDB.Driver.FindOptions<object,object>,System.Threading.CancellationToken)
		// System.Threading.Tasks.Task<MongoDB.Driver.ReplaceOneResult> MongoDB.Driver.IMongoCollectionExtensions.ReplaceOneAsync<object>(MongoDB.Driver.IMongoCollection<object>,System.Linq.Expressions.Expression<System.Func<object,bool>>,object,MongoDB.Driver.ReplaceOptions,System.Threading.CancellationToken)
		// MongoDB.Driver.IMongoCollection<object> MongoDB.Driver.IMongoDatabase.GetCollection<object>(string,MongoDB.Driver.MongoCollectionSettings)
		// object ReferenceCollector.Get<object>(string)
		// object[] System.Array.Empty<object>()
		// int System.HashCode.Combine<object>(object)
		// int System.HashCode.Combine<TrueSync.TSVector2,int>(TrueSync.TSVector2,int)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<object,int>> System.Linq.Enumerable.OrderBy<System.Collections.Generic.KeyValuePair<object,int>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>,System.Func<System.Collections.Generic.KeyValuePair<object,int>,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<object,int>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>,System.Func<System.Collections.Generic.KeyValuePair<object,int>,object>)
		// ET.RpcInfo[] System.Linq.Enumerable.ToArray<ET.RpcInfo>(System.Collections.Generic.IEnumerable<ET.RpcInfo>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Linq.Expressions.Expression<object> System.Linq.Expressions.Expression.Lambda<object>(System.Linq.Expressions.Expression,System.Linq.Expressions.ParameterExpression[])
		// object& System.Runtime.CompilerServices.Unsafe.AsRef<object>(object&)
		// System.Threading.Tasks.Task<object> System.Threading.Tasks.TaskFactory.StartNew<object>(System.Func<object>,System.Threading.CancellationToken)
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Resources.Load<object>(string)
	}
}