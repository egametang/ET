using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ILRuntime.Mono.Cecil;
using System.Reflection;
using ILRuntime.Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Stack;
using ILRuntime.Other;
using ILRuntime.Runtime.Intepreter.RegisterVM;
using System.Threading;

namespace ILRuntime.Runtime.Enviorment
{
    public unsafe delegate StackObject* CLRRedirectionDelegate(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj);
    public delegate object CLRFieldGetterDelegate(ref object target);
    public unsafe delegate StackObject* CLRFieldBindingDelegate(ref object target, ILIntepreter __intp, StackObject* __esp, IList<object> __mStack);
    public delegate void CLRFieldSetterDelegate(ref object target, object value);
    public delegate object CLRMemberwiseCloneDelegate(ref object target);
    public delegate object CLRCreateDefaultInstanceDelegate();
    public delegate object CLRCreateArrayInstanceDelegate(int size);

    public struct TypeSizeInfo
    {
        public ILType Type;
        public int StaticFieldSize;
        public int MethodBodySize;
        public int TotalSize;
    }

    public struct PrewarmInfo
    {
        public string TypeName;
        public string[] MethodNames;
    }
    public class AppDomain
    {
        Queue<ILIntepreter> freeIntepreters = new Queue<ILIntepreter>();
        Dictionary<int, ILIntepreter> intepreters = new Dictionary<int, ILIntepreter>();
        Dictionary<Type, CrossBindingAdaptor> crossAdaptors = new Dictionary<Type, CrossBindingAdaptor>(new ByReferenceKeyComparer<Type>());
        Dictionary<Type, ValueTypeBinder> valueTypeBinders = new Dictionary<Type, ValueTypeBinder>();
        ThreadSafeDictionary<string, IType> mapType = new ThreadSafeDictionary<string, IType>();
        Dictionary<Type, IType> clrTypeMapping = new Dictionary<Type, IType>(new ByReferenceKeyComparer<Type>());
        List<IType> typesByIndex = new List<IType>();
        ThreadSafeDictionary<int, IType> mapTypeToken = new ThreadSafeDictionary<int, IType>();
        ThreadSafeDictionary<int, IMethod> mapMethod = new ThreadSafeDictionary<int, IMethod>();
        ThreadSafeDictionary<long, string> mapString = new ThreadSafeDictionary<long, string>();
        Dictionary<System.Reflection.MethodBase, CLRRedirectionDelegate> redirectMap = new Dictionary<System.Reflection.MethodBase, CLRRedirectionDelegate>();
        Dictionary<System.Reflection.FieldInfo, CLRFieldGetterDelegate> fieldGetterMap = new Dictionary<System.Reflection.FieldInfo, CLRFieldGetterDelegate>();
        Dictionary<System.Reflection.FieldInfo, CLRFieldSetterDelegate> fieldSetterMap = new Dictionary<System.Reflection.FieldInfo, CLRFieldSetterDelegate>();
        Dictionary<System.Reflection.FieldInfo, KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>> fieldBindingMap = new Dictionary<FieldInfo, KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>>();
        Dictionary<Type, CLRMemberwiseCloneDelegate> memberwiseCloneMap = new Dictionary<Type, CLRMemberwiseCloneDelegate>(new ByReferenceKeyComparer<Type>());
        Dictionary<Type, CLRCreateDefaultInstanceDelegate> createDefaultInstanceMap = new Dictionary<Type, CLRCreateDefaultInstanceDelegate>(new ByReferenceKeyComparer<Type>());
        Dictionary<Type, CLRCreateArrayInstanceDelegate> createArrayInstanceMap = new Dictionary<Type, CLRCreateArrayInstanceDelegate>(new ByReferenceKeyComparer<Type>());
        IType voidType, intType, longType, boolType, floatType, doubleType, objectType, jitAttributeType;
        DelegateManager dMgr;
        Assembly[] loadedAssemblies;
        Dictionary<string, byte[]> references = new Dictionary<string, byte[]>();
        DebugService debugService;
        AsyncJITCompileWorker jitWorker = new AsyncJITCompileWorker();
        int defaultJITFlags;

        /// <summary>
        /// Determine if invoking unbinded CLR method(using reflection) is allowed
        /// </summary>
        public bool AllowUnboundCLRMethod { get; set; }

#if DEBUG && !NO_PROFILER
        public int UnityMainThreadID { get; set; }
        public bool IsNotUnityMainThread()
        {
            return UnityMainThreadID != 0 && (UnityMainThreadID != System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
#endif

        internal bool SuppressStaticConstructor { get; set; }

        public int DefaultJITFlags { get { return defaultJITFlags; } }

        public unsafe AppDomain(int defaultJITFlags = ILRuntimeJITFlags.None)
        {
            AllowUnboundCLRMethod = true;
            InvocationContext.InitializeDefaultConverters();
            loadedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var mi = typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray");
            RegisterCLRMethodRedirection(mi, CLRRedirections.InitializeArray);
            mi = typeof(AppDomain).GetMethod("GetCurrentStackTrace");
            RegisterCLRMethodRedirection(mi, CLRRedirections.GetCurrentStackTrace);
            foreach (var i in typeof(System.Activator).GetMethods())
            {
                if (i.Name == "CreateInstance" && i.IsGenericMethodDefinition)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.CreateInstance);
                }
                else if (i.Name == "CreateInstance" && i.GetParameters().Length == 1)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.CreateInstance2);
                }
                else if (i.Name == "CreateInstance" && i.GetParameters().Length == 2)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.CreateInstance3);
                }
            }
            foreach (var i in typeof(System.Type).GetMethods())
            {
                if (i.Name == "GetType" && i.IsStatic)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.GetType);
                }
                if (i.Name == "Equals" && i.GetParameters()[0].ParameterType == typeof(Type))
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.TypeEquals);
                }
                if (i.Name == "IsAssignableFrom" && i.GetParameters()[0].ParameterType == typeof(Type))
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.IsAssignableFrom);
                }
            }
            foreach (var i in typeof(System.Delegate).GetMethods())
            {
                if (i.Name == "Combine" && i.GetParameters().Length == 2)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateCombine);
                }
                if (i.Name == "Remove")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateRemove);
                }
                if (i.Name == "op_Equality")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateEqulity);
                }
                if (i.Name == "op_Inequality")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateInequlity);
                }
            }
            foreach (var i in typeof(MethodBase).GetMethods())
            {
                if (i.Name == "Invoke" && i.GetParameters().Length == 2)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.MethodInfoInvoke);
                }
            }
            foreach (var i in typeof(Enum).GetMethods())
            {
                if (i.Name == "Parse" && i.GetParameters().Length == 2)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumParse);
                }
                if (i.Name == "GetValues" && i.GetParameters().Length == 1)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumGetValues);
                }
                if (i.Name == "GetNames" && i.GetParameters().Length == 1)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumGetNames);
                }
                if (i.Name == "GetName")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumGetName);
                }
#if NET_4_6 || NET_STANDARD_2_0
                if (i.Name == "HasFlag")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumHasFlag);
                }
                if (i.Name == "CompareTo")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumCompareTo);
                }
#endif
                if (i.Name == "ToObject" && i.GetParameters()[1].ParameterType == typeof(int))
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.EnumToObject);
                }
            }
            mi = typeof(System.Type).GetMethod("GetTypeFromHandle");
            RegisterCLRMethodRedirection(mi, CLRRedirections.GetTypeFromHandle);
            mi = typeof(object).GetMethod("GetType");
            RegisterCLRMethodRedirection(mi, CLRRedirections.ObjectGetType);
            mi = typeof(Delegate).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(MethodInfo) });
            RegisterCLRMethodRedirection(mi, CLRRedirections.DelegateCreateDelegate);
            mi = typeof(Delegate).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object), typeof(string) });
            RegisterCLRMethodRedirection(mi, CLRRedirections.DelegateCreateDelegate2);
            mi = typeof(Delegate).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object), typeof(MethodInfo) });
            RegisterCLRMethodRedirection(mi, CLRRedirections.DelegateCreateDelegate3);
            dMgr = new DelegateManager(this);
            dMgr.RegisterDelegateConvertor<Action>((dele) =>
            {
                return dele;
            });

            RegisterCrossBindingAdaptor(new Adapters.AttributeAdapter());

            debugService = new Debugger.DebugService(this);
            this.defaultJITFlags = defaultJITFlags & (ILRuntimeJITFlags.JITImmediately | ILRuntimeJITFlags.JITOnDemand);
        }

        public void Dispose()
        {
            debugService.StopDebugService();
            jitWorker.Dispose();
        }

        public IType VoidType { get { return voidType; } }
        public IType IntType { get { return intType; } }
        public IType LongType { get { return longType; } }
        public IType BoolType { get { return boolType; } }
        public IType FloatType { get { return floatType; } }
        public IType DoubleType { get { return doubleType; } }
        public IType ObjectType { get { return objectType; } }

        public IType JITAttributeType { get { return jitAttributeType; } }

        /// <summary>
        /// Attention, this property isn't thread safe
        /// </summary>
        public Dictionary<string, IType> LoadedTypes { get { return mapType.InnerDictionary; } }

        bool IsThreadBinding = false;
        bool IsBindingDone = false;
        static object bindingLockObject = new object();
        internal Dictionary<MethodBase, CLRRedirectionDelegate> RedirectMap 
        { 
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return redirectMap;
                }
                else
                {
                    lock(bindingLockObject)
                    {
                        return redirectMap;
                    }
                }
            } 
        }
        internal Dictionary<FieldInfo, CLRFieldGetterDelegate> FieldGetterMap
        {
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return fieldGetterMap;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return fieldGetterMap;
                    }
                }
            } 
        }
        internal Dictionary<FieldInfo, CLRFieldSetterDelegate> FieldSetterMap 
        { 
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return fieldSetterMap;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return fieldSetterMap;
                    }
                }
            }
        }
        internal Dictionary<FieldInfo, KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>> FieldBindingMap 
        {
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return fieldBindingMap;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return fieldBindingMap;
                    }
                }
            }
        }
        internal Dictionary<Type, CLRMemberwiseCloneDelegate> MemberwiseCloneMap 
        {
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return memberwiseCloneMap;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return memberwiseCloneMap;
                    }
                }
            }
        }
        internal Dictionary<Type, CLRCreateDefaultInstanceDelegate> CreateDefaultInstanceMap 
        { 
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return createDefaultInstanceMap;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return createDefaultInstanceMap;
                    }
                }
            } 
        }

        internal Dictionary<Type, CLRCreateArrayInstanceDelegate> CreateArrayInstanceMap 
        { 
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return createArrayInstanceMap;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return createArrayInstanceMap;
                    }
                }
            }
        }
        internal Dictionary<Type, CrossBindingAdaptor> CrossBindingAdaptors { get { return crossAdaptors; } }

        internal Dictionary<Type, ValueTypeBinder> ValueTypeBinders 
        { 
            get 
            {
                if (!IsThreadBinding && IsBindingDone)
                {
                    return valueTypeBinders;
                }
                else
                {
                    lock (bindingLockObject)
                    {
                        return valueTypeBinders;
                    }
                }
            }
        }
        public DebugService DebugService { get { return debugService; } }
        internal Dictionary<int, ILIntepreter> Intepreters { get { return intepreters; } }
        internal Queue<ILIntepreter> FreeIntepreters { get { return freeIntepreters; } }

        public DelegateManager DelegateManager { get { return dMgr; } }

        internal void EnqueueJITCompileJob(ILMethod method)
        {
            jitWorker.QueueCompileJob(method);
        }

        /// <summary>
        /// 加载Assembly 文件，从指定的路径
        /// </summary>
        /// <param name="path">路径</param>
        public void LoadAssemblyFile(string path)
        {
            FileInfo file = new FileInfo(path);

            if (!file.Exists)
            {
                throw new FileNotFoundException(string.Format("Assembly File not find!:\r\n{0}", path));
            }
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    LoadAssembly(fs);

                    fs.Dispose();
                }
            }
        }

        public string GetCurrentStackTrace()
        {
            throw new NotSupportedException("Cannot call this method from CLR side");
        }

#if USE_MDB || USE_PDB
        /// <summary>
        /// 加载Assembly 文件和PDB文件或MDB文件，从指定的路径（PDB和MDB文件按默认命名方式，并且和Assembly文件处于同一目录中
        /// </summary>
        /// <param name="path">路径</param>
        public void LoadAssemblyFileAndSymbol(string path)
        {
            FileInfo file = new FileInfo(path);

            if (!file.Exists)
            {
                throw new FileNotFoundException(string.Format("Assembly File not find!:\r\n{0}", path));
            }
            else
            {
                var dlldir = file.DirectoryName;
                var assname = Path.GetFileNameWithoutExtension(file.Name);
                var pdbpath = string.Format("{0}/{1}.pdb",dlldir,assname);
                var mdbpath = string.Format("{0}/{1}.mdb", dlldir, assname);

                string symbolPath = "";

                bool isPDB = true;
                if (File.Exists(pdbpath))
                {
                    symbolPath = pdbpath;
                }
                else if (File.Exists(mdbpath))
                {
                    symbolPath = mdbpath;
                    isPDB = false;
                }


                if (string.IsNullOrEmpty(symbolPath))
                {
                    throw new FileNotFoundException(string.Format("symbol file not find!:\r\ncheck:\r\n{0}\r\n{1}\r\n", pdbpath,mdbpath));
                }

                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {                  
                    
                    using (var pdbfs = new System.IO.FileStream(symbolPath, FileMode.Open))
                    {
                        if (isPDB)
                        {
                            LoadAssemblyPDB(fs, pdbfs);
                        }
                        else
                        {
                            LoadAssemblyMDB(fs, pdbfs);
                        }
                    }
                }
            }
        }
#endif

#if USE_PDB
        /// <summary>
        /// 加载Assembly 文件和PDB文件，两者都从指定的路径
        /// </summary>
        /// <param name="assemblyFilePath">Assembly 文件路径</param>
        /// <param name="symbolFilePath">symbol文件路径</param>
        public void LoadAssemblyFileAndPDB(string assemblyFilePath,string symbolFilePath)
        {
            FileInfo assfile = new FileInfo(assemblyFilePath);
            FileInfo pdbfile = new FileInfo(symbolFilePath);
            if (!assfile.Exists)
            {
                throw new FileNotFoundException(string.Format("Assembly File not find!:\r\n{0}", assemblyFilePath));
            }

            if (!pdbfile.Exists)
            {
                throw new FileNotFoundException(string.Format("symbol file not find!:\r\n{0}", symbolFilePath));
            }

            using (FileStream fs = new FileStream(assfile.FullName, FileMode.Open, FileAccess.Read))
            {

                using (var pdbfs = new System.IO.FileStream(pdbfile.FullName, FileMode.Open))
                {
                    LoadAssemblyPDB(fs, pdbfs);
                }
            }

        }

        /// <summary>
        ///  从流加载Assembly,以及symbol符号文件(pdb)
        /// </summary>
        /// <param name="stream">Assembly Stream</param>
        /// <param name="symbol">PDB Stream</param>
        public void LoadAssemblyPDB(System.IO.Stream stream, System.IO.Stream symbol)
        {
            LoadAssembly(stream, symbol, new Mono.Cecil.Pdb.PdbReaderProvider());
        }

#endif

#if USE_MDB
        /// <summary>
        /// 加载Assembly 文件和MDB文件，两者都从指定的路径
        /// </summary>
        /// <param name="assemblyFilePath">Assembly 文件路径</param>
        /// <param name="symbolFilePath">symbol文件路径</param>
        public void LoadAssemblyFileAndMDB(string assemblyFilePath, string symbolFilePath)
        {
            FileInfo assfile = new FileInfo(assemblyFilePath);
            FileInfo pdbfile = new FileInfo(symbolFilePath);
            if (!assfile.Exists)
            {
                throw new FileNotFoundException(string.Format("Assembly File not find!:\r\n{0}", assemblyFilePath));
            }

            if (!pdbfile.Exists)
            {
                throw new FileNotFoundException(string.Format("symbol file not find!:\r\n{0}", symbolFilePath));
            }

            using (FileStream fs = new FileStream(assfile.FullName, FileMode.Open, FileAccess.Read))
            {

                using (var pdbfs = new System.IO.FileStream(pdbfile.FullName, FileMode.Open))
                {
                    LoadAssemblyMDB(fs, pdbfs);
                }
            }
        }

        /// <summary>
        ///  从流加载Assembly,以及symbol符号文件(Mdb)
        /// </summary>
        /// <param name="stream">Assembly Stream</param>
        /// <param name="symbol">PDB Stream</param>
        public void LoadAssemblyMDB(System.IO.Stream stream, System.IO.Stream symbol)
        {
            LoadAssembly(stream, symbol, new Mono.Cecil.Mdb.MdbReaderProvider());
        }
#endif
        /// <summary>
        /// 从流加载Assembly 不加载symbol符号文件
        /// </summary>
        /// <param name="stream">Dll数据流</param>
        public void LoadAssembly(System.IO.Stream stream)
        {
            LoadAssembly(stream, null, null);
        }

        /// <summary>
        /// 从流加载Assembly,以及symbol符号文件(pdb)
        /// </summary>
        /// <param name="stream">Assembly Stream</param>
        /// <param name="symbol">symbol Stream</param>
        /// <param name="symbolReader">symbol 读取器</param>
        /// <param name="inMemory">是否完整读入内存</param>
        public void LoadAssembly(System.IO.Stream stream, System.IO.Stream symbol, ISymbolReaderProvider symbolReader)
        {
            var module = ModuleDefinition.ReadModule(stream); //从MONO中加载模块

            if (symbolReader != null && symbol != null)
            {
                module.ReadSymbols(symbolReader.GetSymbolReader(module, symbol)); //加载符号表
            }

            if (module.HasAssemblyReferences) //如果此模块引用了其他模块
            {
                /*foreach (var ar in module.AssemblyReferences)
                {
                    if (moduleref.Contains(ar.Name) == false)
                        moduleref.Add(ar.Name);
                    if (moduleref.Contains(ar.FullName) == false)
                        moduleref.Add(ar.FullName);
                }
                */
            }

            if (module.HasTypes)
            {
                List<ILType> types = new List<ILType>();

                foreach (var t in module.GetTypes()) //获取所有此模块定义的类型
                {
                    ILType type = new ILType(t, this);

                    mapType[t.FullName] = type;
                    mapTypeToken[type.GetHashCode()] = type;
                    types.Add(type);

                }
            }

            if (voidType == null)
            {
                voidType = GetType("System.Void");
                intType = GetType("System.Int32");
                longType = GetType("System.Int64");
                boolType = GetType("System.Boolean");
                floatType = GetType("System.Single");
                doubleType = GetType("System.Double");
                objectType = GetType("System.Object");
                jitAttributeType = GetType("ILRuntime.Runtime.ILRuntimeJITAttribute");
            }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            debugService.NotifyModuleLoaded(module.Name);
#endif
        }

        /// <summary>
        /// External reference should be added to the AppDomain by the method
        /// </summary>
        /// <param name="name">Assembly name, without .dll</param>
        /// <param name="content">file content</param>
        public void AddReferenceBytes(string name, byte[] content)
        {
            references[name] = content;
        }

        public void RegisterCLRMethodRedirection(MethodBase mi, CLRRedirectionDelegate func)
        {
            if (mi == null)
                return;

            if (!IsThreadBinding)
            {
                if (!redirectMap.ContainsKey(mi))
                    redirectMap[mi] = func;
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!redirectMap.ContainsKey(mi))
                        redirectMap[mi] = func;
                }
            }
            
        }

        public void RegisterCLRFieldGetter(FieldInfo f, CLRFieldGetterDelegate getter)
        {
            if (!IsThreadBinding)
            {
                if (!fieldGetterMap.ContainsKey(f))
                    fieldGetterMap[f] = getter;
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!fieldGetterMap.ContainsKey(f))
                        fieldGetterMap[f] = getter;
                }
            }
        }

        public void RegisterCLRFieldSetter(FieldInfo f, CLRFieldSetterDelegate setter)
        {
            if (!IsThreadBinding)
            {
                if (!fieldSetterMap.ContainsKey(f))
                    fieldSetterMap[f] = setter;
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!fieldSetterMap.ContainsKey(f))
                        fieldSetterMap[f] = setter;
                }
            }
        }

        public void RegisterCLRFieldBinding(FieldInfo f, CLRFieldBindingDelegate copyToStack, CLRFieldBindingDelegate assignFromStack)
        {
            if (!IsThreadBinding)
            {
                if (!fieldBindingMap.ContainsKey(f))
                    fieldBindingMap[f] = new KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>(copyToStack, assignFromStack);
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!fieldBindingMap.ContainsKey(f))
                        fieldBindingMap[f] = new KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>(copyToStack, assignFromStack);
                }
            }
        }

        public void RegisterCLRMemberwiseClone(Type t, CLRMemberwiseCloneDelegate memberwiseClone)
        {
            if (!IsThreadBinding)
            {
                if (!memberwiseCloneMap.ContainsKey(t))
                    memberwiseCloneMap[t] = memberwiseClone;
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!memberwiseCloneMap.ContainsKey(t))
                        memberwiseCloneMap[t] = memberwiseClone;
                }
            }
        }

        public void RegisterCLRCreateDefaultInstance(Type t, CLRCreateDefaultInstanceDelegate createDefaultInstance)
        {
            if (!IsThreadBinding)
            {
                if (!createDefaultInstanceMap.ContainsKey(t))
                    createDefaultInstanceMap[t] = createDefaultInstance;
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!createDefaultInstanceMap.ContainsKey(t))
                        createDefaultInstanceMap[t] = createDefaultInstance;
                }
            }
        }

        public void RegisterCLRCreateArrayInstance(Type t, CLRCreateArrayInstanceDelegate createArray)
        {
            if (!IsThreadBinding)
            {
                if (!createArrayInstanceMap.ContainsKey(t))
                    createArrayInstanceMap[t] = createArray;
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!createArrayInstanceMap.ContainsKey(t))
                        createArrayInstanceMap[t] = createArray;
                }
            }
        }

        public void RegisterValueTypeBinder(Type t, ValueTypeBinder binder)
        {
            if (!IsThreadBinding)
            {
                if (!valueTypeBinders.ContainsKey(t))
                {
                    valueTypeBinders[t] = binder;
                    binder.RegisterCLRRedirection(this);

                    var ct = GetType(t) as CLRType;
                    binder.CLRType = ct;
                }
            }
            else
            {
                lock (bindingLockObject)
                {
                    if (!valueTypeBinders.ContainsKey(t))
                    {
                        valueTypeBinders[t] = binder;
                        binder.RegisterCLRRedirection(this);

                        var ct = GetType(t) as CLRType;
                        binder.CLRType = ct;
                    }
                }
            }
            
        }

        /// <summary>
        /// 初始化注册Bindings(开启线程做binding没完成时，获取CLR重定向方法会有些消耗)
        /// </summary>
        /// <param name="isThread"></param>
        public void InitializeBindings(bool isThread = false)
        {
            if (IsBindingDone) 
                return;

            IsThreadBinding = isThread;

            if (isThread)
            {
                Thread thread = new Thread(() =>
                {
                    CLRBinding.CLRBindingUtils.Initialize(this);

                    IsBindingDone = true;   //这里线程没有竞争写

#if DEBUG && !NO_PROFILER
                    UnityEngine.Debug.Log("CLRBindingUtils.Initialize Done in thread..");
#endif
                });
                thread.Name = string.Format("CLRBindings-Thread #{0}",thread.ManagedThreadId);
                thread.Start();
            }
            else
            {
                CLRBinding.CLRBindingUtils.Initialize(this);
                IsBindingDone = true;
            }
        }

        /// <summary>
        /// 更近类型名称返回类型
        /// </summary>
        /// <param name="fullname">类型全名 命名空间.类型名</param>
        /// <returns></returns>
        public IType GetType(string fullname)
        {
            IType res;
            if (fullname == null)
            {
                return null;
            }

            if (mapType.TryGetValue(fullname, out res))
                return res;


            string baseType;
            List<string> genericParams;
            bool isArray;
            byte rank;
            ParseGenericType(fullname, out baseType, out genericParams, out isArray, out rank);

            bool isByRef = !string.IsNullOrEmpty(baseType) && baseType[baseType.Length - 1] == '&';
            if (isByRef)
                baseType = baseType.Substring(0, baseType.Length - 1);
            if (genericParams != null || isArray || isByRef)
            {
                IType bt = GetType(baseType);
                if (bt == null)
                {
                    bt = GetType(baseType.Replace("/", "+"));
                }

                if (bt == null)
                    return null;
                if (genericParams != null)
                {
                    KeyValuePair<string, IType>[] genericArguments = new KeyValuePair<string, IType>[genericParams.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        string key = null;
                        if (bt is ILType)
                        {
                            ILType ilt = (ILType)bt;
                            key = ilt.TypeDefinition.GenericParameters[i].FullName;
                        }
                        else
                            key = "!" + i;
                        IType val = GetType(genericParams[i]);
                        if (val == null)
                            return null;
                        genericArguments[i] = new KeyValuePair<string, IType>(key, val);
                    }
                    bt = bt.MakeGenericInstance(genericArguments);
                    mapType[bt.FullName] = bt;
                    mapTypeToken[bt.GetHashCode()] = bt;
                    if (bt is CLRType)
                    {
                        clrTypeMapping[bt.TypeForCLR] = bt;

                        //It still make sense for CLRType, since CLR uses [T] for generics instead of <T>
                        StringBuilder sb = new StringBuilder();
                        sb.Append(baseType);
                        sb.Append('<');
                        for (int i = 0; i < genericParams.Count; i++)
                        {
                            if (i > 0)
                                sb.Append(",");
                            /*if (genericParams[i].Contains(","))
                                sb.Append(genericParams[i].Substring(0, genericParams[i].IndexOf(',')));
                            else*/
                            sb.Append(genericParams[i]);
                        }
                        sb.Append('>');
                        var asmName = sb.ToString();
                        if (bt.FullName != asmName)
                            mapType[asmName] = bt;
                    }
                }

                if (isArray)
                {
                    bt = bt.MakeArrayType(rank);
                    if (bt is CLRType)
                        clrTypeMapping[bt.TypeForCLR] = bt;
                    mapType[bt.FullName] = bt;
                    mapTypeToken[bt.GetHashCode()] = bt;
                    if (!isByRef)
                    {
                        mapType[fullname] = bt;
                        return bt;
                    }
                }

                if (isByRef)
                {
                    res = bt.MakeByRefType();
                    if (bt is CLRType)
                        clrTypeMapping[bt.TypeForCLR] = bt;
                    mapType[fullname] = res;
                    mapType[res.FullName] = res;
                    mapTypeToken[res.GetHashCode()] = res;
                    return res;
                }
                else
                {
                    mapType[fullname] = bt;
                    return bt;
                }
            }
            else
            {
                Type t = Type.GetType(fullname);
                if (t != null)
                {
                    if (!clrTypeMapping.TryGetValue(t, out res))
                    {
                        res = new CLRType(t, this);
                        clrTypeMapping[t] = res;
                    }
                    mapType[fullname] = res;
                    mapType[res.FullName] = res;
                    mapType[t.AssemblyQualifiedName] = res;
                    mapTypeToken[res.GetHashCode()] = res;
                    return res;
                }
            }
            return null;
        }

        internal static void ParseGenericType(string fullname, out string baseType, out List<string> genericParams, out bool isArray, out byte rank)
        {
            StringBuilder sb = new StringBuilder();
            int depth = 0;
            rank = 0;
            baseType = "";
            genericParams = null;

            if (fullname.Length > 2 && fullname[fullname.Length - 2] == '[' && fullname[fullname.Length - 1] == ']')
            {
                fullname = fullname.Substring(0, fullname.Length - 2);
                rank = 1;
                isArray = true;
            }
            else
                isArray = false;
            if (fullname.Length > 2 && fullname[fullname.Length - 2] == '[' && fullname[fullname.Length - 1] == ']')
            {
                baseType = fullname;
                return;
            }
            bool isGenericType = false;
            foreach (var i in fullname)
            {
                if (i == '<' || i == '[')
                {
                    isGenericType = true;
                    break;
                }
            }
            if (isGenericType)
            {
                foreach (var i in fullname)
                {
                    if (i == '<' || i == '[')
                    {
                        depth++;
                        if (depth == 1)
                        {
                            if (isArray && sb.Length == 0)
                            {
                                continue;
                            }
                            else
                            {
                                baseType = sb.ToString();
                                sb.Length = 0;
                                genericParams = new List<string>();
                                continue;
                            }
                        }
                    }
                    if (i == ',' && depth == 1)
                    {
                        string name = sb.ToString();
                        if (name.StartsWith("["))
                            name = name.Substring(1, name.Length - 2);
                        if (!string.IsNullOrEmpty(name))
                            genericParams.Add(name);
                        else
                            ++rank;
                        sb.Length = 0;
                        continue;
                    }
                    if (i == '>' || i == ']')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            string name = sb.ToString();
                            if (name.StartsWith("["))
                                name = name.Substring(1, name.Length - 2);
                            if (!string.IsNullOrEmpty(name))
                                genericParams.Add(name);
                            else if (!string.IsNullOrEmpty(baseType))
                            {
                                if (!isArray)
                                {
                                    isArray = true;
                                    ++rank;
                                }
                                else
                                {
                                    baseType += "[]";
                                }
                            }
                            else
                            {
                                sb.Append("<>");
                                continue;
                            }
                            sb.Length = 0;
                            continue;
                        }
                    }
                    sb.Append(i);
                }
                if (sb.Length > 0)
                {
                    baseType += sb.ToString();
                }
                if (genericParams != null && genericParams.Count == 0)
                    genericParams = null;
            }
            else
                baseType = fullname;
        }

        string GetAssemblyName(IMetadataScope scope)
        {
            return scope is AssemblyNameReference ? ((AssemblyNameReference)scope).FullName : null;
        }

        internal int AllocTypeIndex(IType type)
        {
            lock (typesByIndex)
            {
                int index = typesByIndex.Count;
                typesByIndex.Add(type);
                return index;
            }
        }

        internal IType GetTypeByIndex(int index)
        {
            return typesByIndex[index];
        }

        internal IType GetType(object token, IType contextType, IMethod contextMethod)
        {
            int hash = token.GetHashCode();
            IType res;
            if (mapTypeToken.TryGetValue(hash, out res))
                return res;
            Mono.Cecil.ModuleDefinition module = null;
            KeyValuePair<string, IType>[] genericArguments = null;
            string typename = null;
            string scope = null;
            bool dummyGenericInstance = false;
            if (token is Mono.Cecil.TypeDefinition)
            {
                Mono.Cecil.TypeDefinition _def = (token as Mono.Cecil.TypeDefinition);
                module = _def.Module;
                typename = _def.FullName;
                scope = GetAssemblyName(_def.Scope);
            }
            else if (token is Mono.Cecil.TypeReference)
            {
                Mono.Cecil.TypeReference _ref = (token as Mono.Cecil.TypeReference);
                if (_ref.IsGenericParameter)
                {
                    IType t = null;
                    if (contextType != null)
                    {
                        t = contextType.FindGenericArgument(_ref.Name);
                    }
                    if (t == null && contextMethod != null && contextMethod is ILMethod)
                    {
                        t = ((ILMethod)contextMethod).FindGenericArgument(_ref.Name);
                    }
                    if (t != null)
                    {
                        mapTypeToken[t.GetHashCode()] = t;
                        mapType[t.FullName] = t;
                    }
                    return t;
                }
                if (_ref.IsByReference)
                {
                    var et = ((ByReferenceType)_ref).ElementType;
                    bool valid = !et.ContainsGenericParameter;
                    var t = GetType(et, contextType, contextMethod);
                    if (t != null)
                    {
                        res = t.MakeByRefType();
                        if (res is ILType && valid)
                        {
                            ///Unify the TypeReference
                            ((ILType)res).TypeReference = _ref;
                        }
                        if (valid)
                        {
                            mapTypeToken[hash] = res;
                            mapTypeToken[res.GetHashCode()] = res;
                            if (!string.IsNullOrEmpty(res.FullName))
                                mapType[res.FullName] = res;
                        }
                        return res;
                    }
                    return null;
                }
                if (_ref.IsArray)
                {
                    ArrayType at = (ArrayType)_ref;
                    var t = GetType(at.ElementType, contextType, contextMethod);
                    if (t != null)
                    {
                        res = t.MakeArrayType(at.Rank);
                        if (!_ref.ContainsGenericParameter)
                        {
                            if (res is ILType)
                            {
                                ///Unify the TypeReference
                                ((ILType)res).TypeReference = _ref;
                            }
                            mapTypeToken[hash] = res;
                        }
                        mapTypeToken[res.GetHashCode()] = res;

                        if (!string.IsNullOrEmpty(res.FullName))
                            mapType[res.FullName] = res;
                        return res;
                    }
                    return t;
                }
                module = _ref.Module;
                if (_ref.IsGenericInstance)
                {
                    GenericInstanceType gType = (GenericInstanceType)_ref;
                    typename = gType.ElementType.FullName;
                    scope = GetAssemblyName(gType.ElementType.Scope);
                    TypeReference tr = gType.ElementType;
                    genericArguments = new KeyValuePair<string, IType>[gType.GenericArguments.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        string key = tr.GenericParameters[i].Name;
                        IType val;
                        if (gType.GenericArguments[i].IsGenericParameter)
                        {
                            val = contextType.FindGenericArgument(gType.GenericArguments[i].Name);
                            dummyGenericInstance = true;
                            if (val == null)
                            {
                                if (contextMethod != null && contextMethod is ILMethod)
                                {
                                    val = ((ILMethod)contextMethod).FindGenericArgument(gType.GenericArguments[i].Name);
                                }
                                else
                                    return null;
                            }
                        }
                        else
                            val = GetType(gType.GenericArguments[i], contextType, contextMethod);
                        if (gType.GenericArguments[i].ContainsGenericParameter)
                            dummyGenericInstance = true;
                        if (val != null)
                            genericArguments[i] = new KeyValuePair<string, IType>(key, val);
                        else
                        {
                            if (!dummyGenericInstance)
                                return null;
                            genericArguments = null;
                            break;
                        }
                    }
                }
                else
                {
                    typename = _ref.FullName;
                    scope = GetAssemblyName(_ref.Scope);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            res = GetType(typename);
            if (res == null)
            {
                typename = typename.Replace("/", "+");
                res = GetType(typename);
            }
            if (res == null && scope != null)
                res = GetType(typename + ", " + scope);
            if (res == null)
            {
                if (scope != null)
                {
                    string aname = scope.Split(',')[0];
                    foreach (var i in loadedAssemblies)
                    {
                        if (aname == i.GetName().Name)
                        {
                            res = GetType(typename + ", " + i.FullName);
                            if (res != null)
                                break;
                        }
                    }
                }
                if (res == null)
                {
                    foreach (var j in loadedAssemblies)
                    {
                        res = GetType(typename + ", " + j.FullName);
                        if (res != null)
                            break;
                    }
                }
                if (res != null && scope != null)
                {
                    mapType[typename + ", " + scope] = res;
                }
            }
            if (res == null)
                throw new KeyNotFoundException("Cannot find Type:" + typename);
            if (genericArguments != null)
            {
                res = res.MakeGenericInstance(genericArguments);
                if (!dummyGenericInstance && res is ILType)
                {
                    ((ILType)res).TypeReference = (TypeReference)token;
                }
                if (!string.IsNullOrEmpty(res.FullName))
                {
                    if (res is CLRType || !((ILType)res).TypeReference.HasGenericParameters)
                        mapType[res.FullName] = res;
                }
            }
            mapTypeToken[res.GetHashCode()] = res;
            if (!dummyGenericInstance)
                mapTypeToken[hash] = res;
            return res;
        }

        public IType GetType(int hash)
        {
            IType res;
            if (mapTypeToken.TryGetValue(hash, out res))
                return res;
            else
                return null;
        }

        /// <summary>
        /// 根据CLR类型获取 IL类型
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IType GetType(Type t)
        {
            IType res;
            if (clrTypeMapping.TryGetValue(t, out res))
                return res;
            else
                return GetType(t.AssemblyQualifiedName);
        }

        /// <summary>
        /// Create a instance of the specified type, which is inherited from a CLR Type
        /// </summary>
        /// <typeparam name="T">CLR Type</typeparam>
        /// <param name="type">Full Name of the type</param>
        /// <param name="args">Arguments for the constructor</param>
        /// <returns></returns>
        public T Instantiate<T>(string type, object[] args = null)
        {
            ILTypeInstance ins = Instantiate(type, args);
            return (T)ins.CLRInstance;
        }

        /// <summary>
        /// Create a instance of the specified type
        /// </summary>
        /// <param name="type">Full Name of the type</param>
        /// <param name="args">Arguments for the constructor</param>
        /// <returns></returns>
        public ILTypeInstance Instantiate(string type, object[] args = null)
        {
            IType t;
            if (mapType.TryGetValue(type, out t))
            {
                ILType ilType = t as ILType;
                if (ilType != null)
                {
                    bool hasConstructor = args != null && args.Length != 0;
                    var res = ilType.Instantiate(!hasConstructor);
                    if (hasConstructor)
                    {
                        var ilm = ilType.GetConstructor(args.Length);
                        Invoke(ilm, res, args);
                    }
                    return res;
                }
            }

            return null;
        }

        /// <summary>
        /// Prewarm all methods of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="recursive"></param>
        public void Prewarm(string type, bool recursive = true)
        {
            IType t = GetType(type);
            if (t == null || t is CLRType)
                return;
            var methods = t.GetMethods();
            foreach (var i in methods)
            {
                ((ILMethod)i).Prewarm(recursive);
            }
        }

        /// <summary>
        /// Prewarm all methods specified by the parameter
        /// </summary>
        /// <param name="info"></param>
        /// <param name="recursive"></param>
        public void Prewarm(PrewarmInfo[] info, bool recursive = true)
        {
            foreach(var i in info)
            {
                IType t = GetType(i.TypeName);
                if (t == null || t is CLRType || i.MethodNames == null)
                    continue;
                var methods = t.GetMethods();
                foreach (var mn in i.MethodNames)
                {
                    foreach(var j in methods)
                    {
                        ILMethod m = (ILMethod)j;
                        if(m.Name == mn && m.GenericParameterCount == 0)
                        {
                            m.Prewarm(recursive);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Invoke a method
        /// </summary>
        /// <param name="type">Type's fullname</param>
        /// <param name="method">Method name</param>
        /// <param name="p">Parameters</param>
        /// <returns></returns>
        public object Invoke(string type, string method, object instance, params object[] p)
        {
            IType t = GetType(type);
            if (t == null)
                return null;
            var m = t.GetMethod(method, p != null ? p.Length : 0);
            if (m != null)
            {
                for (int i = 0; i < m.ParameterCount; i++)
                {
                    if (p[i] == null)
                        continue;
                    if (!m.Parameters[i].TypeForCLR.IsAssignableFrom(p[i].GetType()))
                    {
                        throw new ArgumentException("Parameter type mismatch");
                    }
                }
                return Invoke(m, instance, p);
            }
            return null;
        }

        /// <summary>
        /// Invoke a generic method
        /// </summary>
        /// <param name="type">Type's fullname</param>
        /// <param name="method">Method name</param>
        /// <param name="genericArguments">Generic Arguments</param>
        /// <param name="instance">Object Instance of the method</param>
        /// <param name="p">Parameters</param>
        /// <returns></returns>
        public object InvokeGenericMethod(string type, string method, IType[] genericArguments, object instance, params object[] p)
        {
            IType t = GetType(type);
            if (t == null)
                return null;
            var m = t.GetMethod(method, p.Length);

            if (m != null)
            {
                m = m.MakeGenericMethod(genericArguments);
                return Invoke(m, instance, p);
            }
            return null;
        }

        internal ILIntepreter RequestILIntepreter()
        {
            ILIntepreter inteptreter = null;
            lock (freeIntepreters)
            {
                if (freeIntepreters.Count > 0)
                {
                    inteptreter = freeIntepreters.Dequeue();
                    //Clear debug state, because it may be in ShouldBreak State
                    inteptreter.ClearDebugState();
                }
                else
                {
                    inteptreter = new ILIntepreter(this);
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                    intepreters[inteptreter.GetHashCode()] = inteptreter;
                    debugService.ThreadStarted(inteptreter);
#endif
                }
            }

            return inteptreter;
        }

        internal void FreeILIntepreter(ILIntepreter inteptreter)
        {
            lock (freeIntepreters)
            {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                if (inteptreter.CurrentStepType != StepTypes.None)
                {
                    //We should resume all other threads if we are currently doing stepping operation
                    foreach (var i in intepreters)
                    {
                        if (i.Value != inteptreter)
                        {
                            i.Value.ClearDebugState();
                            i.Value.Resume();
                        }
                    }
                    inteptreter.ClearDebugState();
                }
#endif
                inteptreter.Stack.ManagedStack.Clear();
                inteptreter.Stack.Frames.Clear();
                inteptreter.Stack.ClearAllocator();
                freeIntepreters.Enqueue(inteptreter);
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                //debugService.ThreadEnded(inteptreter);
#endif

            }
        }

        /// <summary>
        /// Invokes a specific method
        /// </summary>
        /// <param name="m">Method</param>
        /// <param name="instance">object instance</param>
        /// <param name="p">Parameters</param>
        /// <returns></returns>
        public object Invoke(IMethod m, object instance, params object[] p)
        {
            object res = null;
            if (m is ILMethod)
            {
                ILIntepreter inteptreter = RequestILIntepreter();
                try
                {
                    res = inteptreter.Run((ILMethod)m, instance, p);
                }
                finally
                {
                    FreeILIntepreter(inteptreter);
                }
            }

            return res;
        }

        public InvocationContext BeginInvoke(IMethod m)
        {
            if (m is ILMethod)
            {
                ILIntepreter inteptreter = RequestILIntepreter();
                return new InvocationContext(inteptreter, (ILMethod)m);
            }
            else
                throw new NotSupportedException("Cannot invoke CLRMethod");
        }
        

        bool IsInvalidMethodReference(MethodReference _ref)
        {
            if ((_ref.DeclaringType.Name == "Object" || _ref.DeclaringType.Name == "Attribute")
                    && _ref.Name == ".ctor"
                    && _ref.DeclaringType.Namespace == "System"
                    && _ref.ReturnType.Name == "Void"
                    && _ref.ReturnType.Namespace == "System")
            {
                return true;
            }
            return false;
        }
        
        
        internal IMethod GetMethod(object token, ILType contextType, ILMethod contextMethod, out bool invalidToken)
        {
            string methodname = null;
            string typename = null;
            List<IType> paramList = null;
            int hashCode = token.GetHashCode();
            IMethod method;
            IType[] genericArguments = null;
            IType returnType;
            invalidToken = false;
            bool isConstructor = false;
            if (mapMethod.TryGetValue(hashCode, out method))
                return method;
            IType type = null;
            if (token is Mono.Cecil.MethodReference)
            {
                Mono.Cecil.MethodReference _ref = (token as Mono.Cecil.MethodReference);

                if(IsInvalidMethodReference(_ref))
                {
                    mapMethod[hashCode] = null;
                    return null;
                }
                
                methodname = _ref.Name;
                var typeDef = _ref.DeclaringType;
                type = GetType(typeDef, contextType, contextMethod);
                if (type == null)
                    throw new KeyNotFoundException("Cannot find type:" + typename);

                if (token is Mono.Cecil.MethodDefinition)
                {
                    var def = _ref as MethodDefinition;
                    isConstructor = def.IsConstructor;
                }
                else
                    isConstructor = methodname == ".ctor";

                if (_ref.IsGenericInstance)
                {
                    GenericInstanceMethod gim = (GenericInstanceMethod)_ref;
                    genericArguments = new IType[gim.GenericArguments.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        if (gim.GenericArguments[i].ContainsGenericParameter)
                            invalidToken = true;
                        var gt = GetType(gim.GenericArguments[i], contextType, contextMethod);
                        if (gt == null)
                        {
                            gt = contextMethod.FindGenericArgument(gim.GenericArguments[i].Name);
                            if (gt == null)//This means it contains unresolved generic arguments, which means it's not searching the generic instance
                            {
                                genericArguments = null;
                                break;
                            }
                            else
                                genericArguments[i] = gt;
                        }
                        else
                            genericArguments[i] = gt;
                    }
                }
                if (!invalidToken && typeDef.IsGenericInstance)
                {
                    GenericInstanceType gim = (GenericInstanceType)typeDef;
                    for (int i = 0; i < gim.GenericArguments.Count; i++)
                    {
                        if (gim.GenericArguments[i].ContainsGenericParameter)
                        {
                            invalidToken = true;
                            break;
                        }
                    }
                }
                paramList = _ref.GetParamList(this, contextType, contextMethod, genericArguments);
                returnType = GetType(_ref.ReturnType, type, null);
                if (returnType == null)
                    returnType = GetType(_ref.ReturnType, contextType, null);
            }
            else
            {
                throw new NotImplementedException();
                //Mono.Cecil.GenericInstanceMethod gmethod = _def as Mono.Cecil.GenericInstanceMethod;
                //genlist = new MethodParamList(environment, gmethod);
            }

            if (isConstructor)
                method = type.GetConstructor(paramList);
            else
            {
                method = type.GetMethod(methodname, paramList, genericArguments, returnType, true);
            }

            if (method == null)
            {
                if (isConstructor && contextType.FirstCLRBaseType != null && contextType.FirstCLRBaseType is CrossBindingAdaptor && type.TypeForCLR == ((CrossBindingAdaptor)contextType.FirstCLRBaseType).BaseCLRType)
                {
                    method = contextType.BaseType.GetConstructor(paramList);
                    if (method == null)
                        throw new KeyNotFoundException(string.Format("Cannot find method:{0} in type:{1}, token={2}", methodname, type.FullName, token));
                    invalidToken = true;
                    mapMethod[method.GetHashCode()] = method;
                }
                else
                    throw new KeyNotFoundException(string.Format("Cannot find method:{0} in type:{1}, token={2}", methodname, type.FullName, token));
            }
            if (!invalidToken)
                mapMethod[hashCode] = method;
            else
                mapMethod[method.GetHashCode()] = method;
            return method;
        }

        internal IMethod GetMethod(int tokenHash)
        {
            IMethod res;
            if (mapMethod.TryGetValue(tokenHash, out res))
                return res;

            return null;
        }

        internal long GetStaticFieldIndex(object token, IType contextType, IMethod contextMethod)
        {
            FieldReference f = token as FieldReference;
            var type = GetType(f.DeclaringType, contextType, contextMethod);

            if (type is ILType)
            {
                var it = type as ILType;
                int idx = it.GetFieldIndex(token);
                long res = 0;
                if (it.TypeReference.HasGenericParameters)
                {
                    mapTypeToken[type.GetHashCode()] = it;
                }

                res = ((long)type.GetHashCode() << 32) | (uint)idx;
                return res;
            }
            else
            {
                int idx = type.GetFieldIndex(token);
                long res = ((long)type.GetHashCode() << 32) | (uint)idx;

                return res;
            }
        }

        internal long CacheString(object token)
        {
            long oriHash = token.GetHashCode() & 0xFFFFFFFF;
            long hashCode = oriHash;
            string str = (string)token;
            lock (mapString)
            {
                bool isCollision = CheckStringCollision(hashCode, str);
                long cnt = 0;
                while (isCollision)
                {
                    cnt++;
                    hashCode = cnt << 32 | oriHash;
                    isCollision = CheckStringCollision(hashCode, str);
                }
                mapString[hashCode] = (string)token;
            }
            return hashCode;
        }

        bool CheckStringCollision(long hashCode, string newStr)
        {
            string oldVal;
            if (mapString.TryGetValue(hashCode, out oldVal))
                return oldVal != newStr;
            return false;
        }

        internal string GetString(long hashCode)
        {
            string res = null;
            if (mapString.TryGetValue(hashCode, out res))
                return res;
            return res;
        }

        public void RegisterCrossBindingAdaptor(CrossBindingAdaptor adaptor)
        {
            var bType = adaptor.BaseCLRType;

            if (bType != null)
            {
                if (!crossAdaptors.ContainsKey(bType))
                {
                    var t = adaptor.AdaptorType;
                    var res = GetType(t);
                    if (res == null)
                    {
                        res = new CLRType(t, this);
                        mapType[res.FullName] = res;
                        mapType[t.AssemblyQualifiedName] = res;
                        clrTypeMapping[t] = res;
                    }
                    adaptor.RuntimeType = res;
                    crossAdaptors[bType] = adaptor;
                }
                else
                    throw new Exception("Crossbinding Adapter for " + bType.FullName + " is already added.");
            }
            else
            {
                var bTypes = adaptor.BaseCLRTypes;
                var t = adaptor.AdaptorType;
                var res = GetType(t);
                if (res == null)
                {
                    res = new CLRType(t, this);
                    mapType[res.FullName] = res;
                    mapType[t.AssemblyQualifiedName] = res;
                    clrTypeMapping[t] = res;
                }
                adaptor.RuntimeType = res;

                foreach (var i in bTypes)
                {
                    if (!crossAdaptors.ContainsKey(i))
                    {
                        crossAdaptors[i] = adaptor;
                    }
                    else
                        throw new Exception("Crossbinding Adapter for " + i.FullName + " is already added.");
                }
            }
        }

        public unsafe int GetSizeInMemory(out List<TypeSizeInfo> detail)
        {
            int size = RuntimeStack.MAXIMAL_STACK_OBJECTS * sizeof(StackObject) * (intepreters.Count);
            detail = new List<TypeSizeInfo>();
            HashSet<object> traversed = new HashSet<object>();
            foreach(var i in LoadedTypes)
            {
                ILType type = i.Value as ILType;
                if(type != null)
                {
                    TypeSizeInfo info = new TypeSizeInfo();
                    info.Type = type;
                    info.StaticFieldSize = type.GetStaticFieldSizeInMemory(traversed);
                    info.MethodBodySize = type.GetMethodBodySizeInMemory();
                    info.TotalSize = info.StaticFieldSize + info.MethodBodySize;
                    size += info.TotalSize;
                    detail.Add(info);
                }
            }
            detail.Sort((a, b) => b.TotalSize - a.TotalSize);
            return size;
        }
    }
}
