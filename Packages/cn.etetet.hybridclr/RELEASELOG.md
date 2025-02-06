# 发布日志

发布日期 2024.7.25.

### Runtime

- [new] 支持`Assembly.Load(byte[] assData, byte[] pdbData)`加载dll和pdb符号文件，在2021+版本打印函数堆栈时能显示正确的代码文件和行号
- [fix] 修复团结引擎平台InterpreterImage::GetEventInfo和GetPropertyInfo时有可能未初始化method，导致getter之类的函数为空的bug
- [opt] 优化StackTrace和UnityEngine.Debug打印的函数栈顺序，大多数情况下可以在正确的栈位置地显示解释器函数
- [opt] 优化元数据内存

### Editor

- [fix][严重] 修复生成MethodBridge过程中计算等价类时未考虑到ClassLayout、Layout和FieldOffset因素的bug
- [fix] 修复Library/PlayerDataCache目录不存在时，PatchScriptingAssembliesJsonHook运行异常的bug

发布日期 2024.7.15.

### Runtime

- [opt] 大幅优化metadata元数据内存，内存占用相比6.2.0版本减少了15-40%
- [fix] 修复Transform时未释放IRBasicBlock中insts内存的bug，此bug大约0.7-1.6倍dll大小内存泄露
- [fix] 修复ClassFieldLayoutCalculator内存泄露的bug
- [fix] 修复 MetadataAllocT 错误使用 HYBRIDCLR_MALLOC的bug，正确应该是 HYBRIDCLR_METADATA_MALLOC
- [opt] 优化Interpreter::Execute占用的原生栈大小，避免嵌套过深时出现栈溢出的错误

### Editor

- [fix] 修复 Unity 2022导出的xcode工程包含多个ShellScript片段时错误地删除了非重复片断的bug
- [fix] 修复微信小游戏平台当TextureCompression非默认值时临时目录名为WinxinMiniGame{xxx}，导致没有成功修改scriptingassemblies.json文件的bug
- [fix] 修复团结引擎微信小游戏平台由于同时定义了UNITY_WEIXINMINIGAME和UNITY_WEBGL宏，导致从错误路径查找scriptingassemblies.json文件失败，运行时出现脚本missing的bug

## 6.2.0

发布日期 2024.7.1.

### Runtime

- [merge] 合并2021.3.27f1-2021.3.40f1版本改动
- [opt] 优化metadata元数据内存，内存占用减少了20-25%
- [opt] 优化枚举类型调用GetHashCode的实现，不再产生GC

## 6.1.0

发布日期 2024.6.17.

### Runtime

- [merge] 合并2022.3.23f1-2022.3.33f1版本改动，修复对2022.3.33版本不兼容的问题
- [new] 支持2022.3.33版本新增支持的函数返回值Attribute
- [fix] 修复解释部分FieldInfo调用GetFieldMarshaledSizeForField时崩溃的bug

### Editor

- [fix] 升级dnlib版本，修复ModuleMD保存dll时将未加Assembly限定的mscorlib程序集中类型的程序集设置为当前程序集的严重bug
- [fix] 修复`Generate/LinkXml`生成的link.xml中对UnityEngine.Debug preserve all导致在Unity 2023及更高版本的iOS、visionOS等平台上出现Undefined symbols for architecture arm64: "CheckApplicationIntegrity(IntegrityCheckLevel)" 编译错误的问题。此bug由Unity引起，我们通过在生成link.xml时忽略UnityEngine.Debug类来临时解决这个问题

## 6.0.0

发布日期 2024.6.11.

### Runtime

- [new] 支持Unity 6000.x.y及Unity 2023.2.x版本
- [refactor] 合并ReversePInvokeMethodStub到MethodBridge，同时将MetadataModule中ReversePInvoke相关代码移到InterpreterModule
- [new] 支持MonoPInvokeCallback函数的参数或返回类型为struct类型

### Editor

- [new] 支持Unity 6000.x.y及Unity 2023.2.x版本
- [new] 支持MonoPInvokeCallback函数的参数或返回类型为struct类型
- [new] 新增GeneratedAOTGenericReferenceExcludeExistsAOTClassAndMethods，计算热更新引用的AOT泛型类型和函数时排除掉AOT中已经存在的泛型和函数，最终生成更精准的补充元数据程序集列表
- [fix] 修复在某些不支持visionOS的Unity版本上CopyStrippedAOTAssemblies类有编译错误的bug
- [fix] 修复计算 MonoPInvokeCallback的CallingConvention时，如果delegate在其他程序集中定义，会被错误当作Winapi，导致wrapper签名计算错误的bug
- [fix] PatchScriptingAssemblyList.cs在Unity 2023+版本webgl平台的编译错误
- [fix] 修复计算Native2Manager桥接函数未考虑到MonoPInvokeCallback函数，导致从lua或者其他语言调用c#热更新函数有时候会出现UnsupportedNative2ManagedMethod的bug
- [refactor] 合并ReversePInvokeMethodStub到MethodBridge，同时将MetadataModule中ReversePInvoke相关代码移到InterpreterModule
- [opt] 打包时检查生成桥接函数时的development选项与当前development选项一致。`Generate/All`之后切换development选项再打包，将会产生严重的崩溃
- [opt] `Generate/All`在生成之前检查是否已经安装HybridCLR


## 5.4.1

发布日期 2024.5.30.

### Editor

- [new] 支持visionOS平台
- [fix][**严重**] 修复计算 MonoPInvokeCallback的CallingConvention时，如果delegate在其他程序集中定义，会被错误当作Winapi，导致wrapper签名计算错误的bug
- [fix] 修复tvOS平台使用了错误的Unity-iPhone.xcodeproj路径导致找不到project.pbxproj的bug


## 5.4.0

发布日期 2024.5.20.

### Runtime

- [new] ReversePInvoke支持CallingConvention
- [fix] 修复当参数个数为0时，由于argIdxs未赋值，calli的argBasePtr=argIdx[0]导致函数栈帧指向错误位置的bug
- [fix] 修复MetadataModule::GetReversePInvokeWrappe中ComputeSignature可能死锁的bug
- [fix] 修复AOT基类虚函数implements热更新interface函数时，虚函数调用使用CallInterpVirtual导致运行异常的bug
- [fix] 修复Transform中 PREFIX1前缀指令的子指令有部分缺失并且未按指令号排序的问题
- [fix] 修复no.{x} prefix指令长3字节，但Transform中错误当作2字节处理的bug
- [fix] 修复unaligned.{x} prefix指令长3字节，但Transform中错误当作2字节处理的bug
- [opt] 删除 Interpreter_Execute中不必要的INIT_CLASS操作，因为PREPARE_NEW_FRAME_FROM_NATIVE中一定会检查
- [opt] 不再缓存非泛型函数的MethodBody，优化内存
- [opt] **优化补充元数据内存**，大约节省了2.8倍元数据dll大小的内存
- [refactor] Image::_rawImage字段的类型由RawImage改为RawImage*

### Editor

- [new] ReversePInvoke支持CallingConvention
- [fix] 修复计算struct等价性时，将struct平铺展开计算等价，在某些平台并不适用的bug。例如 struct A { uint8_t x; A2 y; } struct A2 { uint8_t x; int32_t y;}; 跟 struct B {uint8_t x; uint8_t y; int32_t z;} 在x86_64 abi下并不等价
- [fix] 修复当Append xcode项目到现存的xcode项目时，第1次会导致'Run Script'命令被重复追加，从第2次起将会找不到--external-lib-il2-cpp而打印错误日志的bug

## 5.3.0

发布日期 2024.4.22.

### Runtime

- [fix] 修复WebGL平台MachineState::CollectFramesWithoutDuplicates错误地使用hybridclr::metadata::IsInterpreterMethod移除热更新函数，导致补充元数据函数没有移除，StackFrames列表越来越长，打印Stack时死循环的bug。调整实现，统一使用il2cpp::vm::StackTrace::PushFrame及PopFrame实现完美的解释器栈打印。缺点是调用解释器函数增加了维护栈的开销
- [fix] 修复StringUtils::Utf16ToUtf8未正确处理maxinumSize==0，导致InterpreterImage::ConvertConstValue中转换长度为0的string时巨幅溢出的严重bug
- [fix] 修复__ReversePInvokeMethod_XXX函数未设置Il2CppThreadContext，导致从native线程回调时获取Thread变量崩溃的bug
- [merge] 合并 2021.3.34-2021.3.37f1 il2cpp改动
- [merge] 合并 2022.3.19-2022.3.23f1 il2cpp改动

### Editor

- [fix] 修复导出tvOS工程时未修改xcode工程设置，导致打包失败的bug
- [fix] 修复构建tvOS目标时未复制裁剪AOT dll，导致生成桥接函数失败的bug
- [fix] 解决StripAOTDllCommand生成的临时项目的locationPathName不规范导致与某些插件如Embeded Browser不兼容的问题
- [fix] 修复团结引擎1.1.0起删除TUANJIE_2022宏导致没有复制裁剪后的AOT程序集的bug
- [fix] 修复__ReversePInvokeMethod_XXX函数未设置Il2CppThreadContext，导致从native线程回调时获取Thread变量崩溃的bug
- [fix] 修复iOS平台开启development build选项时出现mono相关头文件找不到的bug

## 5.2.1

发布日期 2024.4.7.

### Runtime

- [fix] 修复WebGL平台不打印堆栈日志的bug
- [fix] 修复RuntimeConfig::GetRuntimeOption获取InterpreterThreadExceptionFlowSize错误返回s_threadFrameStackSize的bug

### Editor

- [opt] LoadModule中设置 mod.EnableTypeDefFindCache = true，计算桥接函数的时间缩短为原来的1/3
- [fix] 修复团结引擎导出iOS平台xcode工程文件名改名为Tuanjie-iPhone.xcodeproj导致构建xcode工程失败的bug

## 5.2.0

发布日期 2024.3.25.

### Runtime

- [new] 支持团结引擎
- [new] 支持函数指针，支持IL2CPP_TYPE_FNPTR类型
- [fix] 修复SetMdArrElementVarVar_ref指令未SetWriteBarrier的bug
- [fix] InvokeSingleDelegate当调用一个未补充元数据的泛型函数时，发生崩溃的bug
- [fix] 修复InterpreterDelegateInvoke调用delegate时，如果delegate指向未补充元数据的泛型函数，发生崩溃的bug
- [fix] 修复当BlobStream为空时, RawImage::GetBlobFromRawIndex断言index < _streamBlobHeap.size失败的bug
- [change] 重构metadata index设计，允许分配最多3个64M dll，16个16M dll，64个4M dll，255个1M dll

### Editor

- [new] 支持团结引擎
- [fix] 修复GenericArgumentContext不支持ElementType.FnPtr的bug
- [change] 为RuntimeApi添加[Preserve]特性，避免被裁剪

## 5.1.0

发布日期 2024.2.26.

### Runtime

- [fix] 修复2021未实现System.ByReference`1的.ctor及get_Value函数引发的运行错误的问题，il2cpp通过特殊的instrinct函数实现了正常运行
- [opt] 优化元数据加载，将部分元数据改为延迟加载，大约减少30%的Assembly::Load的执行时间
- [change] tempRet由Interpreter::Execute的局部变量改为CallDelegateInvoke_xxx的局部变量，减少嵌套过深时栈溢出的可能性

## 5.0.0

发布日期 2024.1.26.

### Runtime

- [new] 恢复对2019支持
- [fix] 修复未按依赖顺序加载dll，由于在创建Image时缓存了当时的程序集列表，如果被依赖的程序集在本程序集后加载，延迟访问时由于不在缓存程序集列表而出现TypeLoadedException的bug


### Editor

- [new] 恢复对2019支持
- [new] 支持2019版本在iOS平台以源码形式构建
- [new] 新增 AOTAssemblyMetadataStripper用于剔除AOT dll中非泛型函数元数据
- [new] 新增 MissingMetadataChecker检查裁剪类型或者函数丢失的问题
- [opt] 优化 AOTReference计算，如果泛型的所有泛型参数都是class约束，则不加入到需要补充元数据的集合
- [change] 为了支持团结引擎而作了一些调整（注意，支持团结引擎的il2cpp_plus分支并未公开）

## 4.0.15

发布日期 2024.1.2.

### Runtime

- [fix] 修复计算未完全实例化的泛型类时将VAR和MVAR类型参数大小计算成sizeof(void*)，导致计算出无效且过大的instance，在执行LayoutFieldsLocked过程中调用UpdateInstanceSizeForGenericClass错误地使用泛型基类instance覆盖设置了实例类型的instance值的严重bug
- [change] 支持打印热更新栈，虽然顺序不太正确
- [change] 使用HYBRIDCLR_MALLOC之类分配函数替换IL2CPP_MALLOC
- [refactor] 重构Config接口，统一通过GetRuntimeOption和SetRuntimeOption获取和设置选项
- [opt] 删除NewValueTypeVar和NewValueTypeInterpVar指令不必要的对结构memset操作

### Editor

- [fix] 修复Additional Compiler Arguments中输入 -nullable:enable 之后，Editor抛出InvalidCastException的bug。来自报告 https://github.com/focus-creative-games/hybridclr/issues/116
- [fix] 修复某些情况下报错：BuildFailedException: Build path contains a project previously built without the "Create Visual Studio Solution"
- [opt] 优化桥接函数生成，将同构的struct映射到同一个结构，减少了30-35%的桥接函数数量
- [change] StripAOTDllCommand导出时不再设置BuildScriptsOnly选项
- [change] 调整Installer窗口的显示内容
- [refactor] RuntimeApi中设置hybridclr参数的功能统一通过GetRuntimeOption和SetRuntimeOption函数

## 4.0.14

发布日期 2023.12.11.

### Runtime

- [fix] 修复优化 box; brtrue|brfalse序列时，当类型为class或nullable类型时，无条件转换为无条件branch语句的bug
- [fix] 修复 ClassFieldLayoutCalculator未释放 _classMap的每个key-value对中value对象，造成内存泄露的bug
- [fix] 修复计算 ExplicitLayout的struct的native_size的bug
- [fix] 修复当出现签名完全相同的虚函数与虚泛型函数时，计算override未考虑泛型签名，错误地返回了不匹配的函数，导致虚表错误的bug
- [fix][2021] 修复开启faster(smaller) build选项后某些情况下完全泛型共享AOT函数未使用补充元数据来设置函数指针，导致调用时出错的bug

## 4.0.13

发布日期 2023.11.27.

### Runtime

- [fix] 修复ConvertInvokeArgs有可能传递了非对齐args，导致CopyStackObject在armv7这种要求内存对齐的平台发生崩溃的bug
- [fix] 修复通过StructLayout指定size时，计算ClassFieldLayout的严重bug
- [fix] 修复bgt之类指令未取双重取反进行判断，导致当浮点数与Nan比较时由于不满足对称性执行了错误的分支的bug
- [fix] 修复Class::FromGenericParameter错误地设置了thread_static_fields_size=-1，导致为其分配ThreadStatic内存的严重bug
- [opt] Il2CppGenericInst分配统一使用MetadataCache::GetGenericInst分配唯一池对象，优化内存分配
- [opt] 由于Interpreter部分Il2CppGenericInst统一使用MetadataCache::GetGenericInst，比较 Il2CppGenericContext时直接比较 class_inst和method_inst指针

### Editor

- [fix] 修复裁剪aot dll中出现netstandard时，生成桥接函数异常的bug
- [fix] 修复当出现非常规字段名时生成的桥接函数代码文件有编译错误的bug
- [change] 删除不必要的Datas~/Templates目录，直接以原始文件为模板
- [refactor] 重构 AssemblyCache和 AssemblyReferenceDeepCollector，消除冗余代码

## 4.0.12

发布日期 2023.11.02.

### Editor

- [fix] 修复BashUtil.RemoveDir的bug导致Installer安装失败的bug

## 4.0.11

发布日期 2023.11.02.

### Runtime

- [fix] 修复开启完全泛型共享后, 对于某些MethodInfo，由于methodPointer与virtualMethodPointer使用补充元数据后的解释器函数，而invoker_method仍然为支持完全泛型共享的调用形式，导致invoker_method与methodPointer及virtualMethodPointer不匹配的bug
- [fix] 修复Il2CppGenericContextCompare比较时仅仅对比inst指针的bug，造成热更新模块大量泛型函数重复
- [fix] 修复完全泛型共享时未正确设置MethodInfo的bug

### Editor

- [new] 检查当前安装的libil2cpp版本是否与package版本匹配，避免升级package后未重新install的问题
- [new] Generate支持 netstandard
- [fix] 修复 ReversePInvokeWrap生成不必要地解析referenced dll，导致如果有aot dll引用了netstandard会出现解析错误的bug
- [fix] 修复BashUtil.RemoveDir在偶然情况下出现删除目录失败的问题。新增多次重试
- [fix] 修复桥接函数计算时未归结函数参数类型，导致出现多个同名签名的bug


## 4.0.10

发布日期 2023.10.12.

### Runtime

- [merge][il2cpp] 合并2022.3.10-2022.3.11f1的il2cpp改动，修复2022.3.11版本不兼容的问题

## 4.0.9

发布日期 2023.10.11.

### Runtime

- [merge][il2cpp][fix] 合并2021.3.29-2021.3.31f1的il2cpp改动，修复在2021.3.31版本的不兼容问题
- [merge][il2cpp] 合并2022.3.7-2022.3.10f1的il2cpp改动

### Editor

- [fix] 修复2022版本iOS平台AddLil2cppSourceCodeToXcodeproj2022OrNewer的编译错误

## 4.0.8

发布日期 2023.10.10.

### Runtime

- [fix] 修复计算值类型泛型桥接函数签名时，错误地将值类型泛型参数类型也换成签名，导致与Editor计算的签名不一致的bug
- [fix][refactor] RuntimeApi相关函数由PInvoke改为InternalCall，解决Android平台调用RuntimeApi时触发重新加载libil2cpp.a的问题

### Editor

- [refactor] RuntimeApi相关函数由PInvoke改为InternalCall
- [refactor] 调整HybridCLR.Editor模块一些不规范的命名空间

## 4.0.7

发布日期 2023.10.09.

### Runtime

- [fix] 修复initobj调用了CopyN，但CopyN未考虑对象的内存对齐的情况，在32位这种的平台可能发生未对齐访问异常的bug
- [fix] 修复计算未完全实例化的泛型函数的桥接函数签名时崩溃的bug
- [fix] 修复Il2cpp代码生成选项为faster(smaller)时，2021和2022版本GenericMethod::CreateMethodLocked的bug
- [remove] 移除所有array相关指令中index为int64_t的指令，简化代码
- [remove] 移除ldfld_xxx_ref系列指令

### Editor

- [fix] 修复生成桥接函数时，如果热更新程序集未包含任何代码直接引用了某个aot程序集，则没有为该aot程序集生成桥接函数，导致出现NotSupportNative2Managed异常的bug
- [fix] 修复mac下面路径过长导致拷贝文件失败的bug
- [fix] 修复发布PS5目标时未处理ScriptingAssemblies.json的bug
- [change] 打包时清空裁减aot dll目录

## 4.0.6

发布日期 2023.09.26.

### Runtime

- [fix] 修复2021和2022版本开启完全泛型共享后的bug
- [fix] 修复加载PlaceHolder Assembly后未增加assemblyVersion导致Assembly::GetAssemblies()错误地获得了旧程序集列表的bug

## 4.0.5

发布日期 2023.09.25.

### Runtime

- [fix] 修复Transform中未析构pendingFlows造成内存泄露的bug
- [fix] 修复多维数组SetMdArrElement未区分带ref与不带ref结构的bug
- [fix] 修复CpobjVarVAr_WriteBarrier_n_4未设置size的bug
- [fix] 修复计算interface成员函数slot时未考虑到static之类函数的bug
- [fix] 修复2022版本ExplicitLayout未设置layout.alignment，导致计算出size==0的bug
- [fix] 修复InterpreterInvoke在完全泛型共享时，class类型的methodPointer与virtualMethodPointer有可能不一致，导致失误对this指针+1的bug
- [fix] ldobj当T为byte之类size<4的类型时，未将数据展开为int的bug
- [fix] 修复CopySize未考虑到内存对齐的问题
- [opt] 优化stelem当元素为size较大的struct时统一当作含ref结构的问题
- [opt] TemporaryMemoryArena默认内存块大小由1M调整8K
- [opt] 将Image::Image中Assembly::GetAllAssemblies()换成Assembly::GetAllAssemblies(AssemblyVector&)，避免创建assembly快照而造成不必要的内存泄露

### Editor

- [fix] 修复StandaloneLinux平台DllImport的dllName和裁剪dll路径的错误
- [change] 对于小版本不兼容的Unity版本，不再禁止安装，而是提示警告
- [fix] 修复桥接函数计算中MetaUtil.ToShareTypeSig将Ptr和ByRef计算成IntPtr的bug，正确应该是UIntPtr

## 4.0.4

发布日期 2023.09.11。

### Runtime

- [new][platform] 彻底支持所有平台，包括UWP和PS5
- [fix][严重] 修复计算interpreter部分enum类型的桥接函数签名的bug
- [fix] 修复在某些平台下有编译错误的问题
- [fix] 修复转换STOBJ指令未正确处理增量式GC的bug
- [fix] [fix] 修复 StindVarVar_ref指令未正确设置WriteBarrier的bug
- [fix] 修复2020 GenericMethod::CreateMethodLocked调用vm::MetadataAllocGenericMethod()未持有s_GenericMethodMutex锁的线程安全问题

### Editor

- [fix] 修复AddLil2cppSourceCodeToXcodeproj2021OrOlder在Unity 2020下偶然同时包含了不同目录的两个ThreadPool.cpp文件导致出现编译错误的问题
- [fix] 修复不正确地从EditorUserBuildSettings.selectedBuildTargetGroup获得BuildGroupTarget的bug
- [fix] StripAOTDllCommand生成AOT dll时的BuildOption采用当前Player的设置，避免当打包开启development时，StripAOTDllCommand生成Release aot dll，而打包生成debug aot dll，产生补充元数据及桥接函数生成不匹配的严重错误
- [change] 为了更好地支持全平台，调整了RuntimeApi.cs中dllName的实现，默认取 __Internal
- [change] 为了更好地支持全平台，自2021起裁剪AOT dll全都通过MonoHook复制

## 4.0.3

发布日期 2023.08.31。

### Editor

- [fix] 修复桥接函数计算的bug

## 4.0.2

发布日期 2023.08.29。

### Runtime

- [fix][严重] 修复LdobjVarVar_ref指令的bug。此bug由增量式GC代码引入
- [fix] 修复未处理ResolveField获得的Field为nullptr时情形导致崩溃的bug
- [fix] 修复未正确处理AOT及interpreter interface中显式实现父接口函数的bug

## 4.0.1

发布日期 2023.08.28。

### Runtime

- [fix] 修复2020版本开启增量式GC后出现编译错误的问题

## 4.0.0

发布日期 2023.08.28。

### Runtime

- [new] 支持增量式GC
- [refactor] 重构桥接函数，彻底支持所有il2cpp支持的平台
- [opt] 大幅优化Native2Managed方向的传参

### Editor

- [change] 删除增量式GC选项检查
- [refactor] 重构桥接函数生成

## 3.4.2

发布日期 2023.08.14。

### Runtime

- [fix] 修复RawImage::LoadTables读取_4byteGUIDIndex的bug
- [version] 支持2022.3.7版本
- [version] 支持2021.3.29版本

### Editor

- [fix] 修复计算AOTGenericReference未考虑到泛型调用泛型的情况，导致少计算了泛型及补充元数据

## 3.4.1

发布日期 2023.07.31。

### Runtime

- [fix] 修复 InitializeRuntimeMetadata的内存可见性问题
- [fix] 修复CustomAttribute未正确处理父类NamedArg导致崩溃的bug
- [opt] 优化Transfrom Instinct指令的代码，从HashMap中快速查找而不是挨个匹配

### Editor

- [fix] 修复FilterHotFixAssemblies只对比程序集名尾部，导致有AOT的尾部与某个热更新程序集匹配时意外被过滤的bug
- [change] 检查Settings中热更新程序集列表配置中程序集名不能为空

## 3.4.0

发布日期 2023.07.17。

### Runtime

- [version] 支持2021.3.28和2022.3.4版本
- [opt] 删除MachineState::InitEvalStack分配_StackBase后不必要的memset
- [fix] 修复Exception机制的bug
- [fix] 修复CustomAttribute不支持Type[]类型参数的bug
- [fix] 修复不支持new string(xxx)用法的问题
- [refactor] 重构VTableSetup实现
- [fix] 修复未计算子interface中显式实现父interface的函数的bug
- [opt] Lazy初始化CustomAttributeData，而不是加载时全部初始化，明显减少Assembly.Load时间
- [fix] 修复2022 当new byte\[]{a,b,c...}方式初始化较长的byte[]数据时，返回错误数据的bug

### Editor

- [fix] 修复计算桥接函数未考虑到泛型类的成员函数中可能包含的Native2Managed调用
- [change] link.xml及AOTGenericReferences.cs默认输出路径改为HybridCLRGenerate，避免与顶层HybridCLRData混淆
- [fix] 修复Win下生成的Lump文件中include路径以\为目录分隔符导致同步到Mac后找不到路径的bug
- [refactor] 重构Installer


## 3.3.0 

发布日期 2023.07.03。

### Runtime

- [fix] 修复localloc分配的内存未释放的bug
- [change] MachineState改用RegisterRoot的方式注册执行栈，避免GC时扫描整个堆栈
- [opt] 优化Managed2NativeCallByReflectionInvoke性能，提前计算好传参方式
- [refactor] 重构ConvertInvokeArgs

### Editor

- [fix] 修复2020-2021编译libil2cpp.a未包含brotli相关代码文件导致出现编译错误的bug
- [fix] 修复从导出xcode项目包含绝对路径导致传送到其他机器上编译时找不到路径的bug
- [fix] 解决Generate LinkXml、 MethodBridge、AOTGenericReference、ReversePInvokeWrap 生成不稳定的问题
- [fix] 修复使用不兼容版本打开Installer时出现异常的bug
- [change] 禁用hybridclr后打包ios时不再修改导出的xcode工程

## 3.2.1

### Runtime

- [fix] 修复il2cpp TypeNameParser未将类型名中转义字符'\'去掉，导致找不到嵌套子类型的bug

### Editor

- [new] Installer界面新增显示package版本
- [new] CompileDll新增MacOS、Linux、WebGL目标
- [fix] 修复重构文档站后的帮助文档的链接错误
- [change] 为Anaylizer加上using 限定，解决某些情况下与项目的类型同名而产生编译冲突的问题

## 3.2.0

### Runtime

- [fix] 修复未在PlaceHolder中的Assembly加载时，如果由于不在Assembly列表，也没有任何解释器栈，导致Class::resolve_parse_info_internal查找不到类型的bug
- [fix] 修复读取CustomAttribute System.Type类型数据崩溃的bug

### Editor

- [new] 支持直接从源码打包iOS，不再需要单独编译libil2cpp.a
- [opt] 优化版本不兼容时错误提示，不再抛出异常，而是显示"与当前版本不兼容"


## 3.1.1

### Runtime

- [fix] 修复2021及更高版本，InterpreterModule::Managed2NativeCallByReflectionInvoke调用值类型成员函数时，对this指针多余this=this-1操作。
- [fix] 修复解析CustomAttribute中Enum[]类型字段的bug
- [fix] 修复2021及更高版本反射调用值类型 close Delegate的Invoke函数时未修复target指针的bug
- [new] 新增对增量式GC宏的检查，避免build.gradle中意外开启增量式GC引发的极其隐蔽的问题

### Editor

- [fix] 修复 Win32、Android32、WebGL平台的编译错误
- [fix] 修复计算桥接函数时未考虑到补充元数据泛型实例化会导致访问到一些非公开的函数的情况，导致少生成一些必要的桥接函数
- [opt] 生成AOTGenericReferences时，补充元数据assembly列表由注释改成List<string>列表，方便在代码中直接使用。
- [change] CheckSettings中不再自动设置Api Compatible Level

## 3.1.0

### Runtime

- [rollback] 还原对Unity 2020.3.x支持
- [fix] 修复 WebGL平台ABI的bug

### Editor

- [rollback] 还原对Unity 2020.3.x支持

## 3.0.3

### Runtime

- [fix] 修复Enum::GetValues返回值不正确的bug

## 3.0.2

### Runtime

- [fix] 修复Memory Profiler中创建内存快照时崩溃的bug

### Editor

- [remove] 移除 `HybridCLR/CreateAOTDllSnapshot`菜单


## 3.0.1

### Runtime

- [new] 支持2022.3.0

## 3.0.0

### Runtime

- [fix] 修复不支持访问CustomData字段及值的bug
- [remove] 移除对2019及2020版本支持

### Editor

- 包名更改为com.code-philosophy.hybridclr
- 移除UnityFS插件
- 移除Zip插件
- HybridCLR菜单位置调整

## 2.4.2

### Runtime

- [version] 支持 2020.3.48，最后一个2020LTS版本
- [version] 支持 2021.3.25

## 2.4.1

### Runtime

### Editor

- [fix] 修复遗漏 RELEASELOG.md.meta 文件的问题

## 2.4.0

### Runtime

### Editor

- [new] CheckSettings中检查ScriptingBackend及ApiCompatibleLevel，切换为正确的值
- [new] 新增 MsvcStdextWorkaround.cs 解决2020 vs下stdext编译错误的问题
- [fix] 修复当struct只包含一个float或double字段时，在arm64上计算桥接函数签名错误的bug

## 2.3.1

### Runtime

### Editor

- [fix] 修复本地复制libil2cpp却仍然从仓库下载安装的bug

## 2.3.0

### Runtime

### Editor

- [new] Installer支持从本地目录复制改造后的libil2cpp
- [fix] 修复2019版本MonoBleedingEdge的子目录中包含了过长路径的文件导致Installer复制文件出错的问题


