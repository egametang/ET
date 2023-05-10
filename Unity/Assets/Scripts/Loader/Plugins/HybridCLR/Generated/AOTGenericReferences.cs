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
	// ET.AEvent<object,ET.EventType.EntryEvent3>
	// ET.AEvent<object,ET.EventType.AfterCreateCurrentScene>
	// ET.AEvent<object,ET.EventType.SceneChangeStart>
	// ET.AEvent<object,ET.EventType.SceneChangeFinish>
	// ET.AEvent<object,ET.EventType.LoginFinish>
	// ET.AEvent<object,ET.EventType.AppStartInitFinish>
	// ET.AEvent<object,ET.EventType.AfterUnitCreate>
	// ET.AEvent<object,ET.EventType.AfterCreateClientScene>
	// ET.AEvent<object,ET.EventType.ChangePosition>
	// ET.AEvent<object,ET.EventType.LSAfterUnitCreate>
	// ET.AEvent<object,ET.EventType.LockStepSceneChangeStart>
	// ET.AEvent<object,ET.EventType.LockStepSceneInitFinish>
	// ET.AEvent<object,ET.EventType.EntryEvent2>
	// ET.AEvent<object,ET.Server.EventType.UnitEnterSightRange>
	// ET.AEvent<object,ET.Server.EventType.UnitLeaveSightRange>
	// ET.AEvent<object,ET.EventType.ChangeRotation>
	// ET.AEvent<object,ET.Server.NetInnerComponentOnRead>
	// ET.AEvent<object,ET.Server.NetServerComponentOnRead>
	// ET.AEvent<object,ET.Client.NetClientComponentOnRead>
	// ET.AEvent<object,ET.EventType.NumbericChange>
	// ET.AEvent<object,ET.EventType.EntryEvent1>
	// ET.AInvokeHandler<ET.ConfigComponent.GetAllConfigBytes,object>
	// ET.AInvokeHandler<ET.ConfigComponent.GetOneConfigBytes,object>
	// ET.AInvokeHandler<ET.Server.RobotInvokeArgs,object>
	// ET.AInvokeHandler<ET.NavmeshComponent.RecastFileLoader,object>
	// ET.AMHandler<object>
	// ET.ATimer<object>
	// ET.AwakeSystem<object>
	// ET.AwakeSystem<object,ET.Server.MailboxType>
	// ET.AwakeSystem<object,object>
	// ET.AwakeSystem<object,System.Net.Sockets.AddressFamily>
	// ET.AwakeSystem<object,int>
	// ET.AwakeSystem<object,int,Unity.Mathematics.float3>
	// ET.AwakeSystem<object,long,object>
	// ET.AwakeSystem<object,object,int>
	// ET.AwakeSystem<object,object,object>
	// ET.AwakeSystem<object,object,object,int>
	// ET.ConfigSingleton<object>
	// ET.DestroySystem<object>
	// ET.EntityRef<object>
	// ET.ETAsyncTaskMethodBuilder<uint>
	// ET.ETAsyncTaskMethodBuilder<long>
	// ET.ETAsyncTaskMethodBuilder<int>
	// ET.ETAsyncTaskMethodBuilder<byte>
	// ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>
	// ET.ETAsyncTaskMethodBuilder<object>
	// ET.ETTask<ET.Client.Wait_CreateMyUnit>
	// ET.ETTask<uint>
	// ET.ETTask<ET.WaitType.Wait_Room2C_Start>
	// ET.ETTask<ET.Client.Wait_UnitStop>
	// ET.ETTask<ET.Client.Wait_SceneChangeFinish>
	// ET.ETTask<ET.RobotCase_SecondCaseWait>
	// ET.ETTask<int>
	// ET.ETTask<long>
	// ET.ETTask<System.ValueTuple<uint,object>>
	// ET.ETTask<object>
	// ET.ETTask<byte>
	// ET.IAwake<System.Net.Sockets.AddressFamily>
	// ET.IAwake<ET.Server.MailboxType>
	// ET.IAwake<object>
	// ET.IAwake<int>
	// ET.IAwake<object,int>
	// ET.IAwake<long,object>
	// ET.IAwake<int,Unity.Mathematics.float3>
	// ET.IAwake<object,object>
	// ET.IAwake<object,object,int>
	// ET.LateUpdateSystem<object>
	// ET.ListComponent<long>
	// ET.ListComponent<object>
	// ET.ListComponent<Unity.Mathematics.float3>
	// ET.LoadSystem<object>
	// ET.LSUpdateSystem<object>
	// ET.MultiMap<int,object>
	// ET.Server.AMActorHandler<object,object>
	// ET.Server.AMActorLocationHandler<object,object>
	// ET.Server.AMActorLocationRpcHandler<object,object,object>
	// ET.Server.AMActorRpcHandler<object,object,object>
	// ET.Server.AMRpcHandler<object,object>
	// ET.Singleton<object>
	// ET.UnOrderMultiMap<object,object>
	// ET.UpdateSystem<object>
	// MemoryPack.Formatters.ArrayFormatter<object>
	// MemoryPack.Formatters.ArrayFormatter<ET.LSInput>
	// MemoryPack.Formatters.ArrayFormatter<byte>
	// MemoryPack.Formatters.DictionaryFormatter<long,ET.LSInput>
	// MemoryPack.Formatters.DictionaryFormatter<int,long>
	// MemoryPack.Formatters.ListFormatter<object>
	// MemoryPack.Formatters.ListFormatter<long>
	// MemoryPack.Formatters.ListFormatter<Unity.Mathematics.float3>
	// MemoryPack.IMemoryPackable<object>
	// MemoryPack.IMemoryPackable<ET.LSInput>
	// MemoryPack.MemoryPackFormatter<object>
	// MemoryPack.MemoryPackFormatter<ET.LSInput>
	// MongoDB.Driver.IMongoCollection<object>
	// System.Action<object>
	// System.Action<long,int>
	// System.Action<long,object>
	// System.Action<long,long,object>
	// System.Collections.Generic.Dictionary<int,long>
	// System.Collections.Generic.Dictionary<object,long>
	// System.Collections.Generic.Dictionary<int,ET.RpcInfo>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<ushort,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<uint,object>
	// System.Collections.Generic.Dictionary<long,ET.LSInput>
	// System.Collections.Generic.Dictionary<long,long>
	// System.Collections.Generic.Dictionary<long,ET.EntityRef<object>>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,ET.LSInput>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.HashSet<ushort>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet<long>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet.Enumerator<long>
	// System.Collections.Generic.KeyValuePair<uint,object>
	// System.Collections.Generic.KeyValuePair<long,ET.LSInput>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<int,ET.Server.ActorMessageSender>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<int,long>
	// System.Collections.Generic.List<Unity.Mathematics.float3>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.Enumerator<Unity.Mathematics.float3>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.SortedDictionary<long,object>
	// System.Collections.Generic.SortedDictionary<int,object>
	// System.Collections.Generic.SortedDictionary<object,object>
	// System.Collections.Generic.SortedDictionary<int,ET.Server.ActorMessageSender>
	// System.Collections.Generic.SortedDictionary.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<int,ET.Server.ActorMessageSender>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<long,object>
	// System.Func<object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,int>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,int>,int>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<uint,uint>>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.Task<System.ValueTuple<uint,uint>>
	// System.ValueTuple<uint,object>
	// System.ValueTuple<uint,uint>
	// }}

	public void RefMethods()
	{
		// CommandLine.ParserResult<object> CommandLine.Parser.ParseArguments<object>(System.Collections.Generic.IEnumerable<string>)
		// CommandLine.ParserResult<object> CommandLine.ParserResultExtensions.WithNotParsed<object>(CommandLine.ParserResult<object>,System.Action<System.Collections.Generic.IEnumerable<CommandLine.Error>>)
		// CommandLine.ParserResult<object> CommandLine.ParserResultExtensions.WithParsed<object>(CommandLine.ParserResult<object>,System.Action<object>)
		// object ET.Client.GameObjectHelper.Get<object>(UnityEngine.GameObject,string)
		// object ET.Entity.AddChild<object,object,object>(object,object,bool)
		// object ET.Entity.AddChild<object,int>(int,bool)
		// object ET.Entity.AddChild<object,object>(object,bool)
		// object ET.Entity.AddChild<object,object,object,int>(object,object,int,bool)
		// object ET.Entity.AddChild<object>(bool)
		// object ET.Entity.AddChild<object,long,object>(long,object,bool)
		// object ET.Entity.AddChildWithId<object,object>(long,object,bool)
		// object ET.Entity.AddChildWithId<object>(long,bool)
		// object ET.Entity.AddChildWithId<object,int>(long,int,bool)
		// object ET.Entity.AddComponent<object,ET.Server.MailboxType>(ET.Server.MailboxType,bool)
		// object ET.Entity.AddComponent<object,object,object>(object,object,bool)
		// object ET.Entity.AddComponent<object,int>(int,bool)
		// object ET.Entity.AddComponent<object,object,int>(object,int,bool)
		// object ET.Entity.AddComponent<object>(bool)
		// object ET.Entity.AddComponent<object,System.Net.Sockets.AddressFamily>(System.Net.Sockets.AddressFamily,bool)
		// object ET.Entity.AddComponent<object,int,Unity.Mathematics.float3>(int,Unity.Mathematics.float3,bool)
		// object ET.Entity.AddComponent<object,object>(object,bool)
		// object ET.Entity.GetChild<object>(long)
		// object ET.Entity.GetComponent<object>()
		// object ET.Entity.GetParent<object>()
		// System.Void ET.Entity.RemoveComponent<object>()
		// ET.SceneType ET.EnumHelper.FromString<ET.SceneType>(string)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.PingComponentAwakeSystem.<PingAsync>d__1>(object&,ET.Client.PingComponentAwakeSystem.<PingAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.EnterMapHelper.<EnterMapAsync>d__0>(object&,ET.Client.EnterMapHelper.<EnterMapAsync>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.AI_XunLuo.<Execute>d__1>(object&,ET.Client.AI_XunLuo.<Execute>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.AI_Attack.<Execute>d__1>(object&,ET.Client.AI_Attack.<Execute>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.LoginFinish_CreateLobbyUI.<Run>d__0>(object&,ET.Client.LoginFinish_CreateLobbyUI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.UILobbyComponentSystem.<EnterMap>d__1>(object&,ET.Client.UILobbyComponentSystem.<EnterMap>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0>(object&,ET.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.LoginFinish_RemoveLoginUI.<Run>d__0>(object&,ET.Client.LoginFinish_RemoveLoginUI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.NumericChangeEvent_NotifyWatcher.<Run>d__0>(ET.ETTaskCompleted&,ET.NumericChangeEvent_NotifyWatcher.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.AfterUnitCreate_CreateUnitView.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.AfterUnitCreate_CreateUnitView.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.ChangePosition_SyncGameObjectPos.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.ChangePosition_SyncGameObjectPos.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.ReloadDllConsoleHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.ReloadDllConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.LockStepSceneChangeStart_AddComponent.<Run>d__0>(object&,ET.Client.LockStepSceneChangeStart_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.LockStepSceneInitFinish_Finish.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.LockStepSceneInitFinish_Finish.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.ReloadConfigConsoleHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.ReloadConfigConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.EntryEvent2_InitServer.<Run>d__0>(object&,ET.Server.EntryEvent2_InitServer.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.ConsoleComponentSystem.<Start>d__3>(object&,ET.ConsoleComponentSystem.<Start>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ET.ConsoleComponentSystem.<Start>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ET.ConsoleComponentSystem.<Start>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.BenchmarkClientComponentSystem.<<Start>g__Call|1_0>d>(object&,ET.Server.BenchmarkClientComponentSystem.<<Start>g__Call|1_0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.BenchmarkClientComponentSystem.<Start>d__1>(object&,ET.Server.BenchmarkClientComponentSystem.<Start>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.EntryEvent1_InitShare.<Run>d__0>(ET.ETTaskCompleted&,ET.EntryEvent1_InitShare.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2G_BenchmarkHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2G_BenchmarkHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.C2G_EnterMapHandler.<Run>d__0>(object&,ET.Server.C2G_EnterMapHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Entry.<StartAsync>d__2>(object&,ET.Entry.<StartAsync>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d>(object&,ET.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.UIHelper.<Remove>d__1>(ET.ETTaskCompleted&,ET.Client.UIHelper.<Remove>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.EnterMapHelper.<Match>d__1>(object&,ET.Client.EnterMapHelper.<Match>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.SceneChangeStart_AddComponent.<Run>d__0>(object&,ET.Client.SceneChangeStart_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.MoveHelper.<MoveToAsync>d__1>(object&,ET.Client.MoveHelper.<MoveToAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.M2C_StopHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.M2C_StopHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.RouterAddressComponentSystem.<GetAllRouter>d__2>(object&,ET.Client.RouterAddressComponentSystem.<GetAllRouter>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.RouterAddressComponentSystem.<Init>d__1>(object&,ET.Client.RouterAddressComponentSystem.<Init>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3>(object&,ET.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.M2C_PathfindingResultHandler.<Run>d__0>(object&,ET.Client.M2C_PathfindingResultHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.RouterCheckComponentAwakeSystem.<CheckAsync>d__1>(object&,ET.Client.RouterCheckComponentAwakeSystem.<CheckAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<uint,uint>>,ET.Client.RouterCheckComponentAwakeSystem.<CheckAsync>d__1>(System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<uint,uint>>&,ET.Client.RouterCheckComponentAwakeSystem.<CheckAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.M2C_StartSceneChangeHandler.<Run>d__0>(object&,ET.Client.M2C_StartSceneChangeHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.SceneChangeHelper.<SceneChangeTo>d__0>(object&,ET.Client.SceneChangeHelper.<SceneChangeTo>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.NetClientComponentOnReadEvent.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.NetClientComponentOnReadEvent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.M2C_CreateMyUnitHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.M2C_CreateMyUnitHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.M2C_CreateUnitsHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.M2C_CreateUnitsHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0>(object&,ET.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.M2C_RemoveUnitsHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.M2C_RemoveUnitsHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.LoginHelper.<Login>d__0>(object&,ET.Client.LoginHelper.<Login>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.LSSceneChangeHelper.<SceneChangeTo>d__0>(object&,ET.Client.LSSceneChangeHelper.<SceneChangeTo>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.OneFrameMessagesHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.OneFrameMessagesHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.Room2C_AdjustUpdateTimeHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.Room2C_AdjustUpdateTimeHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.Room2C_EnterMapHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.Room2C_EnterMapHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesComponentSystem.<LoadBundleAsync>d__13>(object&,ET.Client.ResourcesComponentSystem.<LoadBundleAsync>d__13&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesComponentSystem.<LoadOneBundleAllAssets>d__15>(object&,ET.Client.ResourcesComponentSystem.<LoadOneBundleAllAssets>d__15&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesComponentSystem.<UnloadBundleAsync>d__7>(object&,ET.Client.ResourcesComponentSystem.<UnloadBundleAsync>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesLoaderComponentSystem.<LoadAsync>d__1>(object&,ET.Client.ResourcesLoaderComponentSystem.<LoadAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.EntryEvent3_InitClient.<Run>d__0>(object&,ET.Client.EntryEvent3_InitClient.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.AfterCreateClientScene_AddComponent.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.AfterCreateClientScene_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.Match2G_NotifyMatchSuccessHandler.<Run>d__0>(object&,ET.Client.Match2G_NotifyMatchSuccessHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesLoaderComponentSystem.ResourcesLoaderComponentDestroySystem.<>c__DisplayClass0_0.<<Destroy>g__UnLoadAsync|0>d>(object&,ET.Client.ResourcesLoaderComponentSystem.ResourcesLoaderComponentDestroySystem.<>c__DisplayClass0_0.<<Destroy>g__UnLoadAsync|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.LockStepAfterUnitCreate_CreateUnitFView.<Run>d__0>(ET.ETTaskCompleted&,ET.Client.LockStepAfterUnitCreate_CreateUnitFView.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2G_LoginGateHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2G_LoginGateHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<Lock>d__2>(object&,ET.Server.LocationProxyComponentSystem.<Lock>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<AddLocation>d__6>(object&,ET.Server.LocationProxyComponentSystem.<AddLocation>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<Add>d__1>(object&,ET.Server.LocationProxyComponentSystem.<Add>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationOneTypeSystem.<Remove>d__2>(object&,ET.Server.LocationOneTypeSystem.<Remove>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationOneTypeSystem.<Lock>d__3>(object&,ET.Server.LocationOneTypeSystem.<Lock>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationOneTypeSystem.<Add>d__1>(object&,ET.Server.LocationOneTypeSystem.<Add>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.C2G_LoginGateHandler.<Run>d__0>(object&,ET.Server.C2G_LoginGateHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ActorLocationSenderComponentSystem.<SendInner>d__7>(object&,ET.Server.ActorLocationSenderComponentSystem.<SendInner>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorMessage>d__8>(object&,ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorMessage>d__8&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ActorMessageDispatcherComponentHelper.<Handle>d__6>(object&,ET.Server.ActorMessageDispatcherComponentHelper.<Handle>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.MatchComponentSystem.<Match>d__1>(object&,ET.Server.MatchComponentSystem.<Match>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.G2Match_MatchHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.G2Match_MatchHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.Match2Map_GetRoomHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.Match2Map_GetRoomHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.Match2Map_GetRoomHandler.<Run>d__0>(object&,ET.Server.Match2Map_GetRoomHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorRequest>d__7>(object&,ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorRequest>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.FrameMessageHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.FrameMessageHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<RemoveLocation>d__7>(object&,ET.Server.LocationProxyComponentSystem.<RemoveLocation>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ObjectAddRequestHandler.<Run>d__0>(object&,ET.Server.ObjectAddRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ActorMessageSenderComponentSystem.<>c__DisplayClass5_0.<<Send>g__HandleMessageInNextFrame|0>d>(object&,ET.Server.ActorMessageSenderComponentSystem.<>c__DisplayClass5_0.<<Send>g__HandleMessageInNextFrame|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ActorHandleHelper.<>c__DisplayClass0_0.<<Reply>g__HandleMessageInNextFrame|0>d>(object&,ET.Server.ActorHandleHelper.<>c__DisplayClass0_0.<<Reply>g__HandleMessageInNextFrame|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesComponentSystem.<>c__DisplayClass13_0.<<LoadBundleAsync>g__LoadDependency|0>d>(object&,ET.Client.ResourcesComponentSystem.<>c__DisplayClass13_0.<<LoadBundleAsync>g__LoadDependency|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ARobotCase.<Handle>d__1>(object&,ET.Server.ARobotCase.<Handle>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.HttpComponentSystem.<Handle>d__5>(object&,ET.Server.HttpComponentSystem.<Handle>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ET.Server.HttpComponentSystem.<Accept>d__4>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ET.Server.HttpComponentSystem.<Accept>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<UnLock>d__3>(object&,ET.Server.LocationProxyComponentSystem.<UnLock>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ET.Server.DBComponentSystem.<Save>d__12>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ET.Server.DBComponentSystem.<Save>d__12&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ET.Server.DBComponentSystem.<Query>d__6>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ET.Server.DBComponentSystem.<Query>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.DBComponentSystem.<Query>d__6>(object&,ET.Server.DBComponentSystem.<Query>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.ObjectUnLockRequestHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.ObjectUnLockRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ObjectRemoveRequestHandler.<Run>d__0>(object&,ET.Server.ObjectRemoveRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ObjectLockRequestHandler.<Run>d__0>(object&,ET.Server.ObjectLockRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.ObjectGetRequestHandler.<Run>d__0>(object&,ET.Server.ObjectGetRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.DBComponentSystem.<Save>d__12>(object&,ET.Server.DBComponentSystem.<Save>d__12&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.C2Room_ChangeSceneFinishHandler.<Run>d__0>(object&,ET.Server.C2Room_ChangeSceneFinishHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<Remove>d__4>(object&,ET.Server.LocationProxyComponentSystem.<Remove>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.C2G_MatchHandler.<Run>d__0>(object&,ET.Server.C2G_MatchHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.UnitEnterSightRange_NotifyClient.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.UnitEnterSightRange_NotifyClient.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.TransferHelper.<TransferAtFrameFinish>d__0>(object&,ET.Server.TransferHelper.<TransferAtFrameFinish>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.TransferHelper.<Transfer>d__1>(object&,ET.Server.TransferHelper.<Transfer>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.M2M_UnitTransferRequestHandler.<Run>d__0>(object&,ET.Server.M2M_UnitTransferRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3>(object&,ET.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2M_TransferMapHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2M_TransferMapHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.MoveHelper.<FindPathMoveToAsync>d__0>(object&,ET.Server.MoveHelper.<FindPathMoveToAsync>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2M_StopHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2M_StopHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2M_PathfindingResultHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2M_PathfindingResultHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.G2M_SessionDisconnectHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.G2M_SessionDisconnectHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2M_TestRobotCaseHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2M_TestRobotCaseHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.ChangePosition_NotifyAOI.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.ChangePosition_NotifyAOI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.R2G_GetLoginKeyHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.R2G_GetLoginKeyHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.C2G_PingHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.C2G_PingHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.Match2G_NotifyMatchSuccessHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.Match2G_NotifyMatchSuccessHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotCase_FirstCase.<Run>d__0>(object&,ET.Server.RobotCase_FirstCase.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.C2R_LoginHandler.<Run>d__0>(object&,ET.Server.C2R_LoginHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotConsoleHandler.<Run>d__0>(object&,ET.Server.RobotConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotCase_SecondCase.<Run>d__0>(object&,ET.Server.RobotCase_SecondCase.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.CreateRobotConsoleHandler.<Run>d__0>(object&,ET.Server.CreateRobotConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.CreateRobotConsoleHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.CreateRobotConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewRobot>d__1>(object&,ET.Server.RobotCaseSystem.<NewRobot>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewRobot>d__2>(object&,ET.Server.RobotCaseSystem.<NewRobot>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewZoneRobot>d__3>(object&,ET.Server.RobotCaseSystem.<NewZoneRobot>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.NetServerComponentOnReadEvent.<Run>d__0>(object&,ET.Server.NetServerComponentOnReadEvent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewZoneRobot>d__4>(object&,ET.Server.RobotCaseSystem.<NewZoneRobot>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.M2C_TestRobotCase2Handler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.M2C_TestRobotCase2Handler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.RobotConsoleHandler.<Run>d__0>(ET.ETTaskCompleted&,ET.Server.RobotConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.HttpGetRouterHandler.<Handle>d__0>(ET.ETTaskCompleted&,ET.Server.HttpGetRouterHandler.<Handle>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<object,ET.Server.NetInnerComponentOnReadEvent.<Run>d__0>(object&,ET.Server.NetInnerComponentOnReadEvent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.MatchComponentSystem.<Match>d__1>(ET.Server.MatchComponentSystem.<Match>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ActorMessageDispatcherComponentHelper.<Handle>d__6>(ET.Server.ActorMessageDispatcherComponentHelper.<Handle>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.G2Match_MatchHandler.<Run>d__0>(ET.Server.G2Match_MatchHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ActorLocationSenderComponentSystem.<SendInner>d__7>(ET.Server.ActorLocationSenderComponentSystem.<SendInner>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorMessage>d__8>(ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorMessage>d__8&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorRequest>d__7>(ET.Server.ActorMessageDispatcherComponentHelper.<HandleIActorRequest>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationOneTypeSystem.<Add>d__1>(ET.Server.LocationOneTypeSystem.<Add>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.Match2Map_GetRoomHandler.<Run>d__0>(ET.Server.Match2Map_GetRoomHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.NetServerComponentOnReadEvent.<Run>d__0>(ET.Server.NetServerComponentOnReadEvent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2Room_ChangeSceneFinishHandler.<Run>d__0>(ET.Server.C2Room_ChangeSceneFinishHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.Match2G_NotifyMatchSuccessHandler.<Run>d__0>(ET.Server.Match2G_NotifyMatchSuccessHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2G_MatchHandler.<Run>d__0>(ET.Server.C2G_MatchHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationOneTypeSystem.<Remove>d__2>(ET.Server.LocationOneTypeSystem.<Remove>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.NetInnerComponentOnReadEvent.<Run>d__0>(ET.Server.NetInnerComponentOnReadEvent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.HttpGetRouterHandler.<Handle>d__0>(ET.Server.HttpGetRouterHandler.<Handle>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotConsoleHandler.<Run>d__0>(ET.Server.RobotConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotCaseSystem.<NewZoneRobot>d__4>(ET.Server.RobotCaseSystem.<NewZoneRobot>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.FrameMessageHandler.<Run>d__0>(ET.Server.FrameMessageHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationOneTypeSystem.<Lock>d__3>(ET.Server.LocationOneTypeSystem.<Lock>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.HttpComponentSystem.<Accept>d__4>(ET.Server.HttpComponentSystem.<Accept>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationProxyComponentSystem.<Lock>d__2>(ET.Server.LocationProxyComponentSystem.<Lock>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ResourcesComponentSystem.<>c__DisplayClass13_0.<<LoadBundleAsync>g__LoadDependency|0>d>(ET.Client.ResourcesComponentSystem.<>c__DisplayClass13_0.<<LoadBundleAsync>g__LoadDependency|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ActorHandleHelper.<>c__DisplayClass0_0.<<Reply>g__HandleMessageInNextFrame|0>d>(ET.Server.ActorHandleHelper.<>c__DisplayClass0_0.<<Reply>g__HandleMessageInNextFrame|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ActorMessageSenderComponentSystem.<>c__DisplayClass5_0.<<Send>g__HandleMessageInNextFrame|0>d>(ET.Server.ActorMessageSenderComponentSystem.<>c__DisplayClass5_0.<<Send>g__HandleMessageInNextFrame|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d>(ET.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotCaseSystem.<NewZoneRobot>d__3>(ET.Server.RobotCaseSystem.<NewZoneRobot>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ARobotCase.<Handle>d__1>(ET.Server.ARobotCase.<Handle>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.HttpComponentSystem.<Handle>d__5>(ET.Server.HttpComponentSystem.<Handle>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ResourcesLoaderComponentSystem.ResourcesLoaderComponentDestroySystem.<>c__DisplayClass0_0.<<Destroy>g__UnLoadAsync|0>d>(ET.Client.ResourcesLoaderComponentSystem.ResourcesLoaderComponentDestroySystem.<>c__DisplayClass0_0.<<Destroy>g__UnLoadAsync|0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationProxyComponentSystem.<Add>d__1>(ET.Server.LocationProxyComponentSystem.<Add>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.DBComponentSystem.<Save>d__12>(ET.Server.DBComponentSystem.<Save>d__12&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ObjectUnLockRequestHandler.<Run>d__0>(ET.Server.ObjectUnLockRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ObjectRemoveRequestHandler.<Run>d__0>(ET.Server.ObjectRemoveRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ObjectLockRequestHandler.<Run>d__0>(ET.Server.ObjectLockRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ObjectGetRequestHandler.<Run>d__0>(ET.Server.ObjectGetRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationProxyComponentSystem.<RemoveLocation>d__7>(ET.Server.LocationProxyComponentSystem.<RemoveLocation>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationProxyComponentSystem.<AddLocation>d__6>(ET.Server.LocationProxyComponentSystem.<AddLocation>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationProxyComponentSystem.<Remove>d__4>(ET.Server.LocationProxyComponentSystem.<Remove>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.LocationProxyComponentSystem.<UnLock>d__3>(ET.Server.LocationProxyComponentSystem.<UnLock>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.DBComponentSystem.<Query>d__6>(ET.Server.DBComponentSystem.<Query>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotCaseSystem.<NewRobot>d__2>(ET.Server.RobotCaseSystem.<NewRobot>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ObjectAddRequestHandler.<Run>d__0>(ET.Server.ObjectAddRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.CreateRobotConsoleHandler.<Run>d__0>(ET.Server.CreateRobotConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.Match2G_NotifyMatchSuccessHandler.<Run>d__0>(ET.Client.Match2G_NotifyMatchSuccessHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LSSceneChangeHelper.<SceneChangeTo>d__0>(ET.Client.LSSceneChangeHelper.<SceneChangeTo>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.OneFrameMessagesHandler.<Run>d__0>(ET.Client.OneFrameMessagesHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.Room2C_AdjustUpdateTimeHandler.<Run>d__0>(ET.Client.Room2C_AdjustUpdateTimeHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.Room2C_EnterMapHandler.<Run>d__0>(ET.Client.Room2C_EnterMapHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ResourcesComponentSystem.<UnloadBundleAsync>d__7>(ET.Client.ResourcesComponentSystem.<UnloadBundleAsync>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ResourcesComponentSystem.<LoadBundleAsync>d__13>(ET.Client.ResourcesComponentSystem.<LoadBundleAsync>d__13&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ResourcesComponentSystem.<LoadOneBundleAllAssets>d__15>(ET.Client.ResourcesComponentSystem.<LoadOneBundleAllAssets>d__15&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ResourcesLoaderComponentSystem.<LoadAsync>d__1>(ET.Client.ResourcesLoaderComponentSystem.<LoadAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.M2C_RemoveUnitsHandler.<Run>d__0>(ET.Client.M2C_RemoveUnitsHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.EntryEvent3_InitClient.<Run>d__0>(ET.Client.EntryEvent3_InitClient.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0>(ET.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.SceneChangeStart_AddComponent.<Run>d__0>(ET.Client.SceneChangeStart_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0>(ET.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.UIHelper.<Remove>d__1>(ET.Client.UIHelper.<Remove>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LoginFinish_CreateLobbyUI.<Run>d__0>(ET.Client.LoginFinish_CreateLobbyUI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.UILobbyComponentSystem.<EnterMap>d__1>(ET.Client.UILobbyComponentSystem.<EnterMap>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0>(ET.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LoginFinish_RemoveLoginUI.<Run>d__0>(ET.Client.LoginFinish_RemoveLoginUI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.AfterUnitCreate_CreateUnitView.<Run>d__0>(ET.Client.AfterUnitCreate_CreateUnitView.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.AfterCreateClientScene_AddComponent.<Run>d__0>(ET.Client.AfterCreateClientScene_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ChangePosition_SyncGameObjectPos.<Run>d__0>(ET.Client.ChangePosition_SyncGameObjectPos.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.M2C_CreateUnitsHandler.<Run>d__0>(ET.Client.M2C_CreateUnitsHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.NetClientComponentOnReadEvent.<Run>d__0>(ET.Client.NetClientComponentOnReadEvent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.EntryEvent1_InitShare.<Run>d__0>(ET.EntryEvent1_InitShare.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.ConsoleComponentSystem.<Start>d__3>(ET.ConsoleComponentSystem.<Start>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.ReloadConfigConsoleHandler.<Run>d__0>(ET.ReloadConfigConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.ReloadDllConsoleHandler.<Run>d__0>(ET.ReloadDllConsoleHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.NumericChangeEvent_NotifyWatcher.<Run>d__0>(ET.NumericChangeEvent_NotifyWatcher.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.AI_Attack.<Execute>d__1>(ET.Client.AI_Attack.<Execute>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.AI_XunLuo.<Execute>d__1>(ET.Client.AI_XunLuo.<Execute>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.EnterMapHelper.<EnterMapAsync>d__0>(ET.Client.EnterMapHelper.<EnterMapAsync>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.EnterMapHelper.<Match>d__1>(ET.Client.EnterMapHelper.<Match>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.M2C_CreateMyUnitHandler.<Run>d__0>(ET.Client.M2C_CreateMyUnitHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LoginHelper.<Login>d__0>(ET.Client.LoginHelper.<Login>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.M2C_StopHandler.<Run>d__0>(ET.Client.M2C_StopHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.MoveHelper.<MoveToAsync>d__1>(ET.Client.MoveHelper.<MoveToAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.PingComponentAwakeSystem.<PingAsync>d__1>(ET.Client.PingComponentAwakeSystem.<PingAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.RouterAddressComponentSystem.<Init>d__1>(ET.Client.RouterAddressComponentSystem.<Init>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.RouterAddressComponentSystem.<GetAllRouter>d__2>(ET.Client.RouterAddressComponentSystem.<GetAllRouter>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotCaseSystem.<NewRobot>d__1>(ET.Server.RobotCaseSystem.<NewRobot>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.RouterCheckComponentAwakeSystem.<CheckAsync>d__1>(ET.Client.RouterCheckComponentAwakeSystem.<CheckAsync>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.M2C_StartSceneChangeHandler.<Run>d__0>(ET.Client.M2C_StartSceneChangeHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.SceneChangeHelper.<SceneChangeTo>d__0>(ET.Client.SceneChangeHelper.<SceneChangeTo>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.M2C_PathfindingResultHandler.<Run>d__0>(ET.Client.M2C_PathfindingResultHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0>(ET.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3>(ET.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Entry.<StartAsync>d__2>(ET.Entry.<StartAsync>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0>(ET.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.TransferHelper.<Transfer>d__1>(ET.Server.TransferHelper.<Transfer>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.BenchmarkClientComponentSystem.<Start>d__1>(ET.Server.BenchmarkClientComponentSystem.<Start>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.TransferHelper.<TransferAtFrameFinish>d__0>(ET.Server.TransferHelper.<TransferAtFrameFinish>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.BenchmarkClientComponentSystem.<<Start>g__Call|1_0>d>(ET.Server.BenchmarkClientComponentSystem.<<Start>g__Call|1_0>d&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2G_BenchmarkHandler.<Run>d__0>(ET.Server.C2G_BenchmarkHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2G_EnterMapHandler.<Run>d__0>(ET.Server.C2G_EnterMapHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2G_LoginGateHandler.<Run>d__0>(ET.Server.C2G_LoginGateHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.M2M_UnitTransferRequestHandler.<Run>d__0>(ET.Server.M2M_UnitTransferRequestHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2M_TransferMapHandler.<Run>d__0>(ET.Server.C2M_TransferMapHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.MoveHelper.<FindPathMoveToAsync>d__0>(ET.Server.MoveHelper.<FindPathMoveToAsync>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2M_StopHandler.<Run>d__0>(ET.Server.C2M_StopHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2G_PingHandler.<Run>d__0>(ET.Server.C2G_PingHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2M_PathfindingResultHandler.<Run>d__0>(ET.Server.C2M_PathfindingResultHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.G2M_SessionDisconnectHandler.<Run>d__0>(ET.Server.G2M_SessionDisconnectHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2M_TestRobotCaseHandler.<Run>d__0>(ET.Server.C2M_TestRobotCaseHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.ChangePosition_NotifyAOI.<Run>d__0>(ET.Server.ChangePosition_NotifyAOI.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.R2G_GetLoginKeyHandler.<Run>d__0>(ET.Server.R2G_GetLoginKeyHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3>(ET.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.C2R_LoginHandler.<Run>d__0>(ET.Server.C2R_LoginHandler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotCase_FirstCase.<Run>d__0>(ET.Server.RobotCase_FirstCase.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.UnitEnterSightRange_NotifyClient.<Run>d__0>(ET.Server.UnitEnterSightRange_NotifyClient.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LockStepAfterUnitCreate_CreateUnitFView.<Run>d__0>(ET.Client.LockStepAfterUnitCreate_CreateUnitFView.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.M2C_TestRobotCase2Handler.<Run>d__0>(ET.Server.M2C_TestRobotCase2Handler.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LockStepSceneInitFinish_Finish.<Run>d__0>(ET.Client.LockStepSceneInitFinish_Finish.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Client.LockStepSceneChangeStart_AddComponent.<Run>d__0>(ET.Client.LockStepSceneChangeStart_AddComponent.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.RobotCase_SecondCase.<Run>d__0>(ET.Server.RobotCase_SecondCase.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder.Start<ET.Server.EntryEvent2_InitServer.<Run>d__0>(ET.Server.EntryEvent2_InitServer.<Run>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.UIComponentSystem.<Create>d__0>(object&,ET.Client.UIComponentSystem.<Create>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.ActorMessageSenderComponentSystem.<Call>d__8>(object&,ET.Server.ActorMessageSenderComponentSystem.<Call>d__8&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.ActorLocationSenderComponentSystem.<Call>d__10>(object&,ET.Server.ActorLocationSenderComponentSystem.<Call>d__10&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.ResourcesComponentSystem.<LoadOneBundleAsync>d__14>(object&,ET.Client.ResourcesComponentSystem.<LoadOneBundleAsync>d__14&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.ActorLocationSenderComponentSystem.<CallInner>d__11>(object&,ET.Server.ActorLocationSenderComponentSystem.<CallInner>d__11&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.AwaitUnsafeOnCompleted<object,ET.Server.LocationOneTypeSystem.<Get>d__5>(object&,ET.Server.LocationOneTypeSystem.<Get>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.AwaitUnsafeOnCompleted<object,ET.Server.LocationProxyComponentSystem.<Get>d__5>(object&,ET.Server.LocationProxyComponentSystem.<Get>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.SceneFactory.<CreateClientScene>d__0>(ET.ETTaskCompleted&,ET.Client.SceneFactory.<CreateClientScene>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.RobotCaseComponentSystem.<New>d__3>(ET.ETTaskCompleted&,ET.Server.RobotCaseComponentSystem.<New>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.ActorLocationSenderComponentSystem.<Call>d__8>(object&,ET.Server.ActorLocationSenderComponentSystem.<Call>d__8&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.ActorMessageSenderComponentSystem.<Call>d__7>(object&,ET.Server.ActorMessageSenderComponentSystem.<Call>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.RoomManagerComponentSystem.<CreateServerRoom>d__0>(ET.ETTaskCompleted&,ET.Server.RoomManagerComponentSystem.<CreateServerRoom>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.UIHelper.<Create>d__0>(object&,ET.Client.UIHelper.<Create>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.RobotSceneFactory.<Create>d__0>(ET.ETTaskCompleted&,ET.Server.RobotSceneFactory.<Create>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.RobotManagerComponentSystem.<NewRobot>d__0>(object&,ET.Server.RobotManagerComponentSystem.<NewRobot>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewRobot>d__7>(object&,ET.Server.RobotCaseSystem.<NewRobot>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewRobot>d__6>(object&,ET.Server.RobotCaseSystem.<NewRobot>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Client.UILobbyEvent.<OnCreate>d__0>(ET.ETTaskCompleted&,ET.Client.UILobbyEvent.<OnCreate>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.UILobbyEvent.<OnCreate>d__0>(object&,ET.Client.UILobbyEvent.<OnCreate>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Server.RobotCaseSystem.<NewRobot>d__5>(object&,ET.Server.RobotCaseSystem.<NewRobot>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.UILoginEvent.<OnCreate>d__0>(object&,ET.Client.UILoginEvent.<OnCreate>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<ET.ETTaskCompleted,ET.Server.SceneFactory.<CreateServerScene>d__0>(ET.ETTaskCompleted&,ET.Server.SceneFactory.<CreateServerScene>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>.AwaitUnsafeOnCompleted<object,ET.Client.RouterHelper.<GetRouterAddress>d__1>(object&,ET.Client.RouterHelper.<GetRouterAddress>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.UIHelpEvent.<OnCreate>d__0>(object&,ET.Client.UIHelpEvent.<OnCreate>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.RouterHelper.<CreateRouterSession>d__0>(object&,ET.Client.RouterHelper.<CreateRouterSession>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.Client.UIEventComponentSystem.<OnCreate>d__1>(object&,ET.Client.UIEventComponentSystem.<OnCreate>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.SessionSystem.<Call>d__3>(object&,ET.SessionSystem.<Call>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ET.Client.HttpClientHelper.<Get>d__0>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ET.Client.HttpClientHelper.<Get>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,ET.MoveComponentSystem.<MoveToAsync>d__5>(object&,ET.MoveComponentSystem.<MoveToAsync>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<uint>.AwaitUnsafeOnCompleted<object,ET.Client.RouterHelper.<Connect>d__2>(object&,ET.Client.RouterHelper.<Connect>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,ET.SessionSystem.<Call>d__4>(object&,ET.SessionSystem.<Call>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder<int>.AwaitUnsafeOnCompleted<object,ET.Client.MoveHelper.<MoveToAsync>d__0>(object&,ET.Client.MoveHelper.<MoveToAsync>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RobotSceneFactory.<Create>d__0>(ET.Server.RobotSceneFactory.<Create>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.UILobbyEvent.<OnCreate>d__0>(ET.Client.UILobbyEvent.<OnCreate>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<uint>.Start<ET.Client.RouterHelper.<Connect>d__2>(ET.Client.RouterHelper.<Connect>d__2&)
		// System.Void ET.ETAsyncTaskMethodBuilder<System.ValueTuple<uint,object>>.Start<ET.Client.RouterHelper.<GetRouterAddress>d__1>(ET.Client.RouterHelper.<GetRouterAddress>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.UIComponentSystem.<Create>d__0>(ET.Client.UIComponentSystem.<Create>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.RouterHelper.<CreateRouterSession>d__0>(ET.Client.RouterHelper.<CreateRouterSession>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RoomManagerComponentSystem.<CreateServerRoom>d__0>(ET.Server.RoomManagerComponentSystem.<CreateServerRoom>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.SceneFactory.<CreateClientScene>d__0>(ET.Client.SceneFactory.<CreateClientScene>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.Start<ET.Server.LocationOneTypeSystem.<Get>d__5>(ET.Server.LocationOneTypeSystem.<Get>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RobotManagerComponentSystem.<NewRobot>d__0>(ET.Server.RobotManagerComponentSystem.<NewRobot>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.ResourcesComponentSystem.<LoadOneBundleAsync>d__14>(ET.Client.ResourcesComponentSystem.<LoadOneBundleAsync>d__14&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.HttpClientHelper.<Get>d__0>(ET.Client.HttpClientHelper.<Get>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.UIHelper.<Create>d__0>(ET.Client.UIHelper.<Create>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<long>.Start<ET.Server.LocationProxyComponentSystem.<Get>d__5>(ET.Server.LocationProxyComponentSystem.<Get>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<int>.Start<ET.Client.MoveHelper.<MoveToAsync>d__0>(ET.Client.MoveHelper.<MoveToAsync>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.UIEventComponentSystem.<OnCreate>d__1>(ET.Client.UIEventComponentSystem.<OnCreate>d__1&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.SceneFactory.<CreateServerScene>d__0>(ET.Server.SceneFactory.<CreateServerScene>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<byte>.Start<ET.MoveComponentSystem.<MoveToAsync>d__5>(ET.MoveComponentSystem.<MoveToAsync>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RobotCaseSystem.<NewRobot>d__6>(ET.Server.RobotCaseSystem.<NewRobot>d__6&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RobotCaseComponentSystem.<New>d__3>(ET.Server.RobotCaseComponentSystem.<New>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.SessionSystem.<Call>d__4>(ET.SessionSystem.<Call>d__4&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RobotCaseSystem.<NewRobot>d__7>(ET.Server.RobotCaseSystem.<NewRobot>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.ActorLocationSenderComponentSystem.<CallInner>d__11>(ET.Server.ActorLocationSenderComponentSystem.<CallInner>d__11&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.ActorLocationSenderComponentSystem.<Call>d__10>(ET.Server.ActorLocationSenderComponentSystem.<Call>d__10&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.ActorLocationSenderComponentSystem.<Call>d__8>(ET.Server.ActorLocationSenderComponentSystem.<Call>d__8&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.SessionSystem.<Call>d__3>(ET.SessionSystem.<Call>d__3&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.RobotCaseSystem.<NewRobot>d__5>(ET.Server.RobotCaseSystem.<NewRobot>d__5&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.ActorMessageSenderComponentSystem.<Call>d__8>(ET.Server.ActorMessageSenderComponentSystem.<Call>d__8&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Server.ActorMessageSenderComponentSystem.<Call>d__7>(ET.Server.ActorMessageSenderComponentSystem.<Call>d__7&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.UILoginEvent.<OnCreate>d__0>(ET.Client.UILoginEvent.<OnCreate>d__0&)
		// System.Void ET.ETAsyncTaskMethodBuilder<object>.Start<ET.Client.UIHelpEvent.<OnCreate>d__0>(ET.Client.UIHelpEvent.<OnCreate>d__0&)
		// object ET.EventSystem.Invoke<ET.NavmeshComponent.RecastFileLoader,object>(ET.NavmeshComponent.RecastFileLoader)
		// object ET.EventSystem.Invoke<ET.Server.RobotInvokeArgs,object>(int,ET.Server.RobotInvokeArgs)
		// System.Void ET.EventSystem.Publish<object,ET.Server.EventType.UnitEnterSightRange>(object,ET.Server.EventType.UnitEnterSightRange)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.LSAfterUnitCreate>(object,ET.EventType.LSAfterUnitCreate)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.NumbericChange>(object,ET.EventType.NumbericChange)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.ChangePosition>(object,ET.EventType.ChangePosition)
		// System.Void ET.EventSystem.Publish<object,ET.Server.NetInnerComponentOnRead>(object,ET.Server.NetInnerComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.MoveStart>(object,ET.EventType.MoveStart)
		// System.Void ET.EventSystem.Publish<object,ET.Server.NetServerComponentOnRead>(object,ET.Server.NetServerComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.MoveStop>(object,ET.EventType.MoveStop)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.ChangeRotation>(object,ET.EventType.ChangeRotation)
		// System.Void ET.EventSystem.Publish<object,ET.Client.NetClientComponentOnRead>(object,ET.Client.NetClientComponentOnRead)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.SceneChangeStart>(object,ET.EventType.SceneChangeStart)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.EnterMapFinish>(object,ET.EventType.EnterMapFinish)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.SceneChangeFinish>(object,ET.EventType.SceneChangeFinish)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.AfterCreateClientScene>(object,ET.EventType.AfterCreateClientScene)
		// System.Void ET.EventSystem.Publish<object,ET.Server.EventType.UnitLeaveSightRange>(object,ET.Server.EventType.UnitLeaveSightRange)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.AfterCreateCurrentScene>(object,ET.EventType.AfterCreateCurrentScene)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.AfterUnitCreate>(object,ET.EventType.AfterUnitCreate)
		// System.Void ET.EventSystem.Publish<object,ET.EventType.LockStepSceneInitFinish>(object,ET.EventType.LockStepSceneInitFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.LoginFinish>(object,ET.EventType.LoginFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.LockStepSceneChangeStart>(object,ET.EventType.LockStepSceneChangeStart)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.EntryEvent2>(object,ET.EventType.EntryEvent2)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.EntryEvent3>(object,ET.EventType.EntryEvent3)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.AppStartInitFinish>(object,ET.EventType.AppStartInitFinish)
		// ET.ETTask ET.EventSystem.PublishAsync<object,ET.EventType.EntryEvent1>(object,ET.EventType.EntryEvent1)
		// object ET.Game.AddSingleton<object>()
		// object ET.JsonHelper.FromJson<object>(string)
		// object ET.LSEntity.AddComponent<object>(bool)
		// object ET.MongoHelper.Deserialize<object>(byte[])
		// System.Void ET.MongoHelper.RegisterStruct<ET.LSInput>()
		// object ET.NetServices.FetchMessage<object>()
		// System.Void ET.ObjectHelper.Swap<object>(object&,object&)
		// System.Void ET.ObjectWaitSystem.Notify<ET.Client.Wait_SceneChangeFinish>(ET.ObjectWait,ET.Client.Wait_SceneChangeFinish)
		// System.Void ET.ObjectWaitSystem.Notify<ET.RobotCase_SecondCaseWait>(ET.ObjectWait,ET.RobotCase_SecondCaseWait)
		// System.Void ET.ObjectWaitSystem.Notify<ET.Client.Wait_UnitStop>(ET.ObjectWait,ET.Client.Wait_UnitStop)
		// System.Void ET.ObjectWaitSystem.Notify<ET.Client.Wait_CreateMyUnit>(ET.ObjectWait,ET.Client.Wait_CreateMyUnit)
		// System.Void ET.ObjectWaitSystem.Notify<ET.WaitType.Wait_Room2C_Start>(ET.ObjectWait,ET.WaitType.Wait_Room2C_Start)
		// ET.ETTask<ET.Client.Wait_CreateMyUnit> ET.ObjectWaitSystem.Wait<ET.Client.Wait_CreateMyUnit>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.Client.Wait_SceneChangeFinish> ET.ObjectWaitSystem.Wait<ET.Client.Wait_SceneChangeFinish>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.WaitType.Wait_Room2C_Start> ET.ObjectWaitSystem.Wait<ET.WaitType.Wait_Room2C_Start>(ET.ObjectWait,ET.ETCancellationToken)
		// ET.ETTask<ET.Client.Wait_UnitStop> ET.ObjectWaitSystem.Wait<ET.Client.Wait_UnitStop>(ET.ObjectWait,ET.ETCancellationToken)
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
		// object MemoryPack.MemoryPackReader.ReadPackable<object>()
		// System.Void MemoryPack.MemoryPackReader.ReadPackable<object>(object&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int,int>(int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,long>(byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int>(byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long>(byte&,int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long,long>(byte&,int&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long,long,int>(byte&,int&,int&,long&,long&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long,long,long>(byte&,int&,int&,long&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long>(byte&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,long>(byte&,int&,int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte>(byte&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<ET.LSInput>(ET.LSInput&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int>(byte&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int,long>(int&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<uint>(uint&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.float3>(Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.quaternion>(Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<Unity.Mathematics.quaternion,int>(Unity.Mathematics.quaternion&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,uint>(byte&,uint&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,Unity.Mathematics.float3>(byte&,int&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,ET.LSInput>(byte&,int&,long&,ET.LSInput&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,Unity.Mathematics.float3>(byte&,long&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<TrueSync.TSQuaternion>(TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<TrueSync.TSVector>(TrueSync.TSVector&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,TrueSync.TSVector,TrueSync.TSQuaternion>(byte&,long&,TrueSync.TSVector&,TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,Unity.Mathematics.float3,Unity.Mathematics.quaternion>(byte&,int&,long&,Unity.Mathematics.float3&,Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,long,int,int,Unity.Mathematics.float3,Unity.Mathematics.float3>(byte&,long&,int&,int&,Unity.Mathematics.float3&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanagedArray<byte>(byte[]&)
		// byte[] MemoryPack.MemoryPackReader.ReadUnmanagedArray<byte>()
		// object MemoryPack.MemoryPackReader.ReadValue<object>()
		// System.Void MemoryPack.MemoryPackReader.ReadValue<object>(object&)
		// System.Void MemoryPack.MemoryPackWriter.WritePackable<object>(object&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<Unity.Mathematics.quaternion,int>(Unity.Mathematics.quaternion&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<ET.LSInput>(ET.LSInput&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanaged<int,long>(int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedArray<byte>(byte[])
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,Unity.Mathematics.float3,Unity.Mathematics.quaternion>(byte,byte&,int&,long&,Unity.Mathematics.float3&,Unity.Mathematics.quaternion&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,Unity.Mathematics.float3>(byte,byte&,long&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long>(byte,byte&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,Unity.Mathematics.float3>(byte,byte&,int&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int>(byte,byte&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<int,int>(byte,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long,long,long>(byte,byte&,int&,int&,long&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,uint>(byte,byte&,uint&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte>(byte,byte&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,long>(byte,byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long,ET.LSInput>(byte,byte&,int&,long&,ET.LSInput&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int>(byte,byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,long>(byte,byte&,int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,TrueSync.TSVector,TrueSync.TSQuaternion>(byte,byte&,long&,TrueSync.TSVector&,TrueSync.TSQuaternion&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long,long>(byte,byte&,int&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long,long,int>(byte,byte&,int&,int&,long&,long&,int&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,int,int,long>(byte,byte&,int&,int&,long&)
		// System.Void MemoryPack.MemoryPackWriter.WriteUnmanagedWithObjectHeader<byte,long,int,int,Unity.Mathematics.float3,Unity.Mathematics.float3>(byte,byte&,long&,int&,int&,Unity.Mathematics.float3&,Unity.Mathematics.float3&)
		// System.Void MemoryPack.MemoryPackWriter.WriteValue<object>(object&)
		// System.Threading.Tasks.Task<object> MongoDB.Driver.IAsyncCursorExtensions.FirstOrDefaultAsync<object>(MongoDB.Driver.IAsyncCursor<object>,System.Threading.CancellationToken)
		// System.Threading.Tasks.Task<MongoDB.Driver.IAsyncCursor<object>> MongoDB.Driver.IMongoCollectionExtensions.FindAsync<object>(MongoDB.Driver.IMongoCollection<object>,System.Linq.Expressions.Expression<System.Func<object,bool>>,MongoDB.Driver.FindOptions<object,object>,System.Threading.CancellationToken)
		// System.Threading.Tasks.Task<MongoDB.Driver.ReplaceOneResult> MongoDB.Driver.IMongoCollectionExtensions.ReplaceOneAsync<object>(MongoDB.Driver.IMongoCollection<object>,System.Linq.Expressions.Expression<System.Func<object,bool>>,object,MongoDB.Driver.ReplaceOptions,System.Threading.CancellationToken)
		// MongoDB.Driver.IMongoCollection<object> MongoDB.Driver.IMongoDatabase.GetCollection<object>(string,MongoDB.Driver.MongoCollectionSettings)
		// object ReferenceCollector.Get<object>(string)
		// object[] System.Array.Empty<object>()
		// int System.HashCode.Combine<TrueSync.TSVector2,int>(TrueSync.TSVector2,int)
		// int System.HashCode.Combine<int,object>(int,object)
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