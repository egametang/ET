using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"MemoryPack.dll",
		"System.Core.dll",
		"System.Runtime.CompilerServices.Unsafe.dll",
		"System.dll",
		"Unity.Core.dll",
		"Unity.Loader.dll",
		"Unity.ThirdParty.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// ET.AEvent<object,ET.ChangePosition>
	// ET.AEvent<object,ET.ChangeRotation>
	// ET.AEvent<object,ET.Client.AfterCreateClientScene>
	// ET.AEvent<object,ET.Client.AfterCreateCurrentScene>
	// ET.AEvent<object,ET.Client.AfterUnitCreate>
	// ET.AEvent<object,ET.Client.AppStartInitFinish>
	// ET.AEvent<object,ET.Client.LSSceneChangeStart>
	// ET.AEvent<object,ET.Client.LSSceneInitFinish>
	// ET.AEvent<object,ET.Client.LoginFinish>
	// ET.AEvent<object,ET.Client.SceneChangeFinish>
	// ET.AEvent<object,ET.Client.SceneChangeStart>
	// ET.AEvent<object,ET.EntryEvent1>
	// ET.AEvent<object,ET.EntryEvent3>
	// ET.AEvent<object,ET.NumbericChange>
	// ET.AInvokeHandler<ET.FiberInit,object>
	// ET.AInvokeHandler<ET.MailBoxInvoker>
	// ET.AInvokeHandler<ET.NetOuterComponentOnRead>
	// ET.ATimer<object>
	// ET.AwakeSystem<object,System.Net.Sockets.AddressFamily>
	// ET.AwakeSystem<object,int>
	// ET.AwakeSystem<object,object,int>
	// ET.AwakeSystem<object,object,object>
	// ET.AwakeSystem<object,object>
	// ET.AwakeSystem<object>
	// ET.DestroySystem<object>
	// ET.DoubleMap<object,long>
	// ET.ETAsyncTaskMethodBuilder<ET.Client.WaitType.Wait_Room2C_Start>
	// ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_CreateMyUnit>
	// ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_SceneChangeFinish>
	// ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_UnitStop>
	// ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>
	// ET.ETAsyncTaskMethodBuilder<byte>
	// ET.ETAsyncTaskMethodBuilder<int>
	// ET.ETAsyncTaskMethodBuilder<long>
	// ET.ETAsyncTaskMethodBuilder<object>
	// ET.ETAsyncTaskMethodBuilder<uint>
	// ET.ETTask<ET.Client.WaitType.Wait_Room2C_Start>
	// ET.ETTask<ET.Client.Wait_CreateMyUnit>
	// ET.ETTask<ET.Client.Wait_SceneChangeFinish>
	// ET.ETTask<ET.Client.Wait_UnitStop>
	// ET.ETTask<System.ValueTuple<uint,object>>
	// ET.ETTask<byte>
	// ET.ETTask<int>
	// ET.ETTask<long>
	// ET.ETTask<object>
	// ET.ETTask<uint>
	// ET.EntityRef<object>
	// ET.IAwake<System.Net.Sockets.AddressFamily>
	// ET.IAwake<int>
	// ET.IAwake<object,int>
	// ET.IAwake<object,object>
	// ET.IAwake<object>
	// ET.LateUpdateSystem<object>
	// ET.ListComponent<Unity.Mathematics.float3>
	// ET.ListComponent<object>
	// ET.Singleton<object>
	// ET.UnOrderMultiMap<object,object>
	// ET.UpdateSystem<object>
	// MemoryPack.Formatters.ArrayFormatter<ET.LSInput>
	// MemoryPack.Formatters.ArrayFormatter<byte>
	// MemoryPack.Formatters.ArrayFormatter<object>
	// MemoryPack.Formatters.DictionaryFormatter<int,long>
	// MemoryPack.Formatters.DictionaryFormatter<long,ET.LSInput>
	// MemoryPack.Formatters.ListFormatter<Unity.Mathematics.float3>
	// MemoryPack.Formatters.ListFormatter<long>
	// MemoryPack.Formatters.ListFormatter<object>
	// MemoryPack.IMemoryPackable<ET.LSInput>
	// MemoryPack.IMemoryPackable<object>
	// MemoryPack.MemoryPackFormatter<ET.LSInput>
	// MemoryPack.MemoryPackFormatter<object>
	// System.Action<long,ET.ActorId,object>
	// System.Action<long,int>
	// System.Action<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,ET.LSInput>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,ET.RpcInfo>
	// System.Collections.Generic.Dictionary<int,long>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,ET.EntityRef<object>>
	// System.Collections.Generic.Dictionary<long,ET.LSInput>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<ushort,object>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet<ushort>
	// System.Collections.Generic.KeyValuePair<int,long>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<long,ET.LSInput>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<Unity.Mathematics.float3>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Unity.Mathematics.float3>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.SortedDictionary.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<int,object>
	// System.Collections.Generic.SortedDictionary<int,object>
	// System.Collections.Generic.SortedDictionary<long,object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,int>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<object,int>,object>
	// System.Func<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.Task<object>
	// System.ValueTuple<uint,object>
	// System.ValueTuple<uint,uint>
	// }}

	public void RefMethods()
	{
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,object>(ET.ETTaskCompleted&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.WaitType.Wait_Room2C_Start>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_CreateMyUnit>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_SceneChangeFinish>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_UnitStop>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<int>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,object>(ET.ETTaskCompleted&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<uint>.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.WaitType.Wait_Room2C_Start>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_CreateMyUnit>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_SceneChangeFinish>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<ET.Client.Wait_UnitStop>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<byte>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<int>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<object>(object&)
		// System.Void ET.ETAsyncTaskMethodBuilder<uint>.Start<object>(object&)
		// object ET.Entity.AddChild<object,object,object>(object,object,bool)
		// object ET.Entity.AddChildWithId<object,int>(long,int,bool)
		// object ET.Entity.AddChildWithId<object,object>(long,object,bool)
		// object ET.Entity.AddChildWithId<object>(long,bool)
		// object ET.Entity.AddComponent<object,ET.MailBoxType>(ET.MailBoxType,bool)
		// object ET.Entity.AddComponent<object,System.Net.Sockets.AddressFamily>(System.Net.Sockets.AddressFamily,bool)
		// object ET.Entity.AddComponent<object,object,int>(object,int,bool)
		// object ET.Entity.AddComponent<object>(bool)
		// object ET.Entity.AddComponentWithId<object>(long,bool)
		// object ET.Entity.GetChild<object>(long)
		// object ET.Entity.GetComponent<object>()
		// object ET.Entity.GetParent<object>()
		// System.Void ET.Entity.RemoveComponent<object>()
		// ET.SceneType ET.EnumHelper.FromString<ET.SceneType>(string)
		// System.Void ET.EventSystem.Invoke<ET.NetOuterComponentOnRead>(long,ET.NetOuterComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.ChangePosition>(object,ET.ChangePosition)
		// System.Void ET.EventSystem.Publish<object,ET.ChangeRotation>(object,ET.ChangeRotation)
		// System.Void ET.EventSystem.Publish<object,ET.Client.AfterCreateCurrentScene>(object,ET.Client.AfterCreateCurrentScene)
		// System.Void ET.EventSystem.Publish<object,ET.Client.AfterUnitCreate>(object,ET.Client.AfterUnitCreate)
		// System.Void ET.EventSystem.Publish<object,ET.Client.EnterMapFinish>(object,ET.Client.EnterMapFinish)
		// System.Void ET.EventSystem.Publish<object,ET.Client.LSSceneInitFinish>(object,ET.Client.LSSceneInitFinish)
		// System.Void ET.EventSystem.Publish<object,ET.Client.SceneChangeFinish>(object,ET.Client.SceneChangeFinish)
		// System.Void ET.EventSystem.Publish<object,ET.Client.SceneChangeStart>(object,ET.Client.SceneChangeStart)
		// System.Void ET.EventSystem.Publish<object,ET.MoveStart>(object,ET.MoveStart)
		// System.Void ET.EventSystem.Publish<object,ET.MoveStop>(object,ET.MoveStop)
		// System.Void ET.EventSystem.Publish<object,ET.NumbericChange>(object,ET.NumbericChange)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.Client.AppStartInitFinish>(object,ET.Client.AppStartInitFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.Client.LSSceneChangeStart>(object,ET.Client.LSSceneChangeStart)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.Client.LoginFinish>(object,ET.Client.LoginFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EntryEvent1>(object,ET.EntryEvent1)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EntryEvent2>(object,ET.EntryEvent2)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EntryEvent3>(object,ET.EntryEvent3)
		// object ET.MongoHelper.FromJson<object>(string)
		// System.Void ET.MongoHelper.RegisterStruct<ET.LSInput>()
		// System.Void ET.ObjectHelper.Swap<object>(object&,object&)
		// object ET.ObjectPool.Fetch<object>()
		// System.Void ET.RandomGenerator.BreakRank<object>(System.Collections.Generic.List<object>)
		// string ET.StringHelper.ArrayToString<float>(float[])
		// object ET.World.AddSingleton<object>()
		// System.Collections.Generic.List<object> MemoryPack.Formatters.ListFormatter.DeserializePackable<object>(MemoryPack.MemoryPackReader&)
		// System.Void MemoryPack.Formatters.ListFormatter.DeserializePackable<object>(MemoryPack.MemoryPackReader&,System.Collections.Generic.List<object>&)
		// System.Void MemoryPack.Formatters.ListFormatter.SerializePackable<object>(MemoryPack.MemoryPackWriter&,System.Collections.Generic.List<object>&)
		// bool MemoryPack.MemoryPackFormatterProvider.IsRegistered<ET.LSInput>()
		// bool MemoryPack.MemoryPackFormatterProvider.IsRegistered<object>()
		// System.Void MemoryPack.MemoryPackFormatterProvider.Register<ET.LSInput>(MemoryPack.MemoryPackFormatter<ET.LSInput>)
		// System.Void MemoryPack.MemoryPackFormatterProvider.Register<object>(MemoryPack.MemoryPackFormatter<object>)
		// System.Void MemoryPack.MemoryPackReader.ReadPackable<object>(object&)
		// object MemoryPack.MemoryPackReader.ReadPackable<object>()
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<ET.ActorId>(ET.ActorId&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<ET.LSInput>(ET.LSInput&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<TrueSync.TSQuaternion>(TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<TrueSync.TSVector>(TrueSync.TSVector&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.float3>(Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.quaternion,int>(Unity.Mathematics.quaternion&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.quaternion>(Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,ET.ActorId>(byte&,int&,ET.ActorId&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,Unity.Mathematics.float3>(byte&,int&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int>(byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,ET.LSInput>(byte&,int&,long&,ET.LSInput&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,Unity.Mathematics.float3,Unity.Mathematics.quaternion>(byte&,int&,long&,Unity.Mathematics.float3&,Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,long>(byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int>(byte&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,TrueSync.TSVector,TrueSync.TSQuaternion>(byte&,long&,TrueSync.TSVector&,TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,Unity.Mathematics.float3>(byte&,long&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,int,int,Unity.Mathematics.float3,Unity.Mathematics.float3>(byte&,long&,int&,int&,Unity.Mathematics.float3&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,int,long>(byte&,long&,int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long>(byte&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,uint>(byte&,uint&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte>(byte&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<uint>(uint&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanagedArray<byte>(byte[]&)
		// byte[] MemoryPack.MemoryPackReader.ReadUnmanagedArray<byte>()
		// System.Void MemoryPack.MemoryPackReader.ReadValue<object>(object&)
		// object MemoryPack.MemoryPackReader.ReadValue<object>()
		// System.Void MemoryPack.MemoryPackWriter.WritePackable<object>(object&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<ET.LSInput>(ET.LSInput&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<Unity.Mathematics.quaternion,int>(Unity.Mathematics.quaternion&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedArray<byte>(byte[])
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,ET.ActorId>(byte,byte&,int&,ET.ActorId&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,Unity.Mathematics.float3>(byte,byte&,int&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int>(byte,byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,ET.LSInput>(byte,byte&,int&,long&,ET.LSInput&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,Unity.Mathematics.float3,Unity.Mathematics.quaternion>(byte,byte&,int&,long&,Unity.Mathematics.float3&,Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,long>(byte,byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int>(byte,byte&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,TrueSync.TSVector,TrueSync.TSQuaternion>(byte,byte&,long&,TrueSync.TSVector&,TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,Unity.Mathematics.float3>(byte,byte&,long&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,int,int,Unity.Mathematics.float3,Unity.Mathematics.float3>(byte,byte&,long&,int&,int&,Unity.Mathematics.float3&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,int,long>(byte,byte&,long&,int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long>(byte,byte&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,uint>(byte,byte&,uint&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte>(byte,byte&)
		// System.Void MemoryPack.MemoryPackWriter.WriteValue<object>(object&)
		// object ReferenceCollector.Get<object>(string)
		// ET.Client.WaitType.Wait_Room2C_Start System.Activator.CreateInstance<ET.Client.WaitType.Wait_Room2C_Start>()
		// ET.Client.Wait_CreateMyUnit System.Activator.CreateInstance<ET.Client.Wait_CreateMyUnit>()
		// ET.Client.Wait_SceneChangeFinish System.Activator.CreateInstance<ET.Client.Wait_SceneChangeFinish>()
		// ET.Client.Wait_UnitStop System.Activator.CreateInstance<ET.Client.Wait_UnitStop>()
		// object[] System.Array.Empty<object>()
		// int System.HashCode.Combine<TrueSync.TSVector2,int>(TrueSync.TSVector2,int)
		// int System.HashCode.Combine<object>(object)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<object,int>> System.Linq.Enumerable.OrderBy<System.Collections.Generic.KeyValuePair<object,int>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>,System.Func<System.Collections.Generic.KeyValuePair<object,int>,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<object,int>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>,System.Func<System.Collections.Generic.KeyValuePair<object,int>,object>)
		// ET.RpcInfo[] System.Linq.Enumerable.ToArray<ET.RpcInfo>(System.Collections.Generic.IEnumerable<ET.RpcInfo>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// object& System.Runtime.CompilerServices.Unsafe.AsRef<object>(object&)
		// System.Threading.Tasks.Task<object> System.Threading.Tasks.TaskFactory.StartNew<object>(System.Func<object>,System.Threading.CancellationToken)
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
	}
}