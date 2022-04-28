using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.CLR.TypeSystem
{
    public class ILType : IType
    {
        Dictionary<string, List<ILMethod>> methods;
        TypeReference typeRef;
        TypeDefinition definition;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        bool staticConstructorCalled;
        ILMethod staticConstructor;
        List<ILMethod> constructors;
        IType [] fieldTypes;
        FieldDefinition [] fieldDefinitions;
        IType [] staticFieldTypes;
        FieldDefinition [] staticFieldDefinitions;
        Dictionary<string, int> fieldMapping;
        Dictionary<string, int> staticFieldMapping;
        ILTypeStaticInstance staticInstance;
        Dictionary<int, int> fieldTokenMapping = new Dictionary<int, int> ();
        int fieldStartIdx = -1;
        int totalFieldCnt = -1;
        KeyValuePair<string, IType> [] genericArguments;
        IType baseType, byRefType, enumType, elementType;
        Dictionary<int, IType> arrayTypes;
        Type arrayCLRType, byRefCLRType;
        IType [] interfaces;
        bool baseTypeInitialized = false;
        bool interfaceInitialized = false;
        List<ILType> genericInstances;
        bool isDelegate;
        ILRuntimeType reflectionType;
        ILType genericDefinition;
        IType firstCLRBaseType, firstCLRInterface;
        int hashCode = -1;
        int tIdx = -1;
        static int instance_id = 0x10000000;
        int jitFlags;
        public TypeDefinition TypeDefinition { get { return definition; } }
        bool mToStringGot, mEqualsGot, mGetHashCodeGot;
        IMethod mToString, mEquals, mGetHashCode;
        int valuetypeFieldCount, valuetypeManagedCount;
        bool valuetypeSizeCalculated;

        public IMethod ToStringMethod
        {
            get
            {
                if ( !mToStringGot )
                {
                    IMethod m = appdomain.ObjectType.GetMethod ( "ToString", 0, true );
                    mToString = GetVirtualMethod ( m );
                    mToStringGot = true;
                }
                return mToString;
            }
        }

        public IMethod EqualsMethod
        {
            get
            {
                if ( !mEqualsGot )
                {
                    IMethod m = appdomain.ObjectType.GetMethod ( "Equals", 1, true );
                    mEquals = GetVirtualMethod ( m );
                    mEqualsGot = true;
                }
                return mEquals;
            }
        }

        public IMethod GetHashCodeMethod
        {
            get
            {
                if ( !mGetHashCodeGot )
                {
                    IMethod m = appdomain.ObjectType.GetMethod ( "GetHashCode", 0, true );
                    mGetHashCode = GetVirtualMethod ( m );
                    mGetHashCodeGot = true;
                }
                return mGetHashCode;
            }
        }

        public TypeReference TypeReference
        {
            get { return typeRef; }
            set
            {
                typeRef = value;
                RetriveDefinitino ( value );
            }
        }

        public IType BaseType
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                return baseType;
            }
        }

        public IType [] Implements
        {
            get
            {
                if ( !interfaceInitialized )
                    InitializeInterfaces ();
                return interfaces;
            }
        }

        public ILTypeStaticInstance StaticInstance
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                if ( methods == null )
                    InitializeMethods ();
                if ( staticInstance == null && staticFieldTypes != null )
                {
                    staticInstance = new ILTypeStaticInstance ( this );
                }
                if ( staticInstance != null && !staticConstructorCalled )
                {
                    staticConstructorCalled = true;
                    if ( staticConstructor != null && ( !TypeReference.HasGenericParameters || IsGenericInstance ) )
                    {
                        appdomain.Invoke ( staticConstructor, null, null );
                    }
                }
                return staticInstance;
            }
        }

        public IType [] FieldTypes
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                return fieldTypes;
            }
        }

        public IType [] StaticFieldTypes
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                return staticFieldTypes;
            }
        }

        public FieldDefinition [] StaticFieldDefinitions
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                return staticFieldDefinitions;
            }
        }

        public Dictionary<string, int> FieldMapping
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields (); return fieldMapping;
            }
        }

        public IType FirstCLRBaseType
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                return firstCLRBaseType;
            }
        }

        public IType FirstCLRInterface
        {
            get
            {
                if ( !interfaceInitialized )
                    InitializeInterfaces ();
                return firstCLRInterface;
            }
        }
        public bool HasGenericParameter
        {
            get
            {
                return typeRef.HasGenericParameters && genericArguments == null;
            }
        }

        public bool IsGenericParameter
        {
            get
            {
                return typeRef.IsGenericParameter && genericArguments == null;
            }
        }

        public Dictionary<string, int> StaticFieldMapping { get { return staticFieldMapping; } }
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get
            {
                return appdomain;
            }
        }

        internal int FieldStartIndex
        {
            get
            {
                if ( fieldStartIdx < 0 )
                {
                    if ( BaseType != null )
                    {
                        if ( BaseType is ILType )
                        {
                            fieldStartIdx = ( ( ILType ) BaseType ).TotalFieldCount;
                        }
                        else
                            fieldStartIdx = 0;
                    }
                    else
                        fieldStartIdx = 0;
                }
                return fieldStartIdx;
            }
        }

        public int TotalFieldCount
        {
            get
            {
                if ( totalFieldCnt < 0 )
                {
                    if ( fieldMapping == null )
                        InitializeFields ();
                    if ( BaseType != null )
                    {
                        if ( BaseType is ILType )
                        {
                            totalFieldCnt = ( ( ILType ) BaseType ).TotalFieldCount + fieldTypes.Length;
                        }
                        else
                            totalFieldCnt = fieldTypes.Length;
                    }
                    else
                        totalFieldCnt = fieldTypes.Length;
                }
                return totalFieldCnt;
            }
        }

        internal List<ILType> GenericInstances
        {
            get
            {
                return genericInstances;
            }
        }

        /// <summary>
        /// 初始化IL类型
        /// </summary>
        /// <param name="def">MONO返回的类型定义</param>
        /// <param name="domain">ILdomain</param>
        public ILType ( TypeReference def, Runtime.Enviorment.AppDomain domain )
        {
            this.typeRef = def;
            RetriveDefinitino ( def );
            appdomain = domain;
            jitFlags = domain.DefaultJITFlags;
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="def"></param>
        void RetriveDefinitino ( TypeReference def )
        {
            if ( !def.IsGenericParameter && definition == null )
            {
                if ( def is TypeSpecification )
                {
                    if ( def.IsByReference || def is ArrayType )
                    {
                        definition = null;
                    }
                    else
                        RetriveDefinitino ( ( ( TypeSpecification ) def ).ElementType );
                }
                else
                    definition = def as TypeDefinition;
            }
        }

        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }

        public ILType GetGenericDefinition ()
        {
            return genericDefinition;
        }
        public KeyValuePair<string, IType> [] GenericArguments
        {
            get
            {
                return genericArguments;
            }
        }

        public IType ElementType { get { return elementType; } }

        public bool IsArray
        {
            get; private set;
        }

        public int ArrayRank
        {
            get; private set;
        }

        public bool IsByRef
        {
            get
            {
                return typeRef.IsByReference;
            }
        }

        private bool? isValueType;

        public bool IsValueType
        {
            get
            {
                if ( IsArray )
                    return false;
                if ( isValueType == null )
                    isValueType = definition.IsValueType;

                return isValueType.Value;
            }
        }

        public bool IsDelegate
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                return isDelegate;
            }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public bool IsInterface
        {
            get
            {
                return TypeDefinition.IsInterface;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                if ( typeRef is ArrayType )
                {
                    return arrayCLRType;
                }
                else if ( typeRef is ByReferenceType )
                {
                    return byRefCLRType;
                }
                else if ( this.IsEnum )
                {
                    if ( enumType == null )
                        InitializeFields ();
                    return enumType.TypeForCLR;
                }
                else if ( FirstCLRBaseType != null && FirstCLRBaseType is CrossBindingAdaptor )
                {
                    return ( ( CrossBindingAdaptor ) FirstCLRBaseType ).RuntimeType.TypeForCLR;
                }
                else if ( FirstCLRInterface != null && FirstCLRInterface is CrossBindingAdaptor )
                {
                    return ( ( CrossBindingAdaptor ) FirstCLRInterface ).RuntimeType.TypeForCLR;
                }
                else
                    return typeof ( ILTypeInstance );
            }
        }

        public Type ReflectionType
        {
            get
            {
                if ( reflectionType == null )
                    reflectionType = new ILRuntimeType ( this );
                return reflectionType;
            }
        }

        public IType ByRefType
        {
            get
            {
                return byRefType;
            }
        }
        public IType ArrayType
        {
            get
            {
                return arrayTypes != null ? arrayTypes [ 1 ] : null;
            }
        }

        public bool IsEnum
        {
            get
            {
                return definition != null ? definition.IsEnum : false;
            }
        }

        string fullName, fullNameForNested;

        public string FullNameForNested
        {
            get
            {
                if ( string.IsNullOrEmpty ( fullNameForNested ) )
                {
                    if ( typeRef.IsNested )
                    {
                        fullNameForNested = FullName.Replace ( "/", "." );
                    }
                    else
                        fullNameForNested = FullName;
                }
                return fullNameForNested;
            }
        }

        public string FullName
        {
            get
            {
                if ( string.IsNullOrEmpty ( fullName ) )
                {
                    if ( typeRef.HasGenericParameters && genericArguments != null )
                    {
                        StringBuilder sb = new StringBuilder ();
                        sb.Append ( typeRef.FullName );
                        sb.Append ( '<' );
                        bool first = true;
                        foreach ( var i in genericArguments )
                        {
                            if ( first )
                                first = false;
                            else
                                sb.Append ( ", " );
                            sb.Append ( i.Value.FullName );
                        }
                        sb.Append ( '>' );
                        fullName = sb.ToString ();
                    }
                    else
                        fullName = typeRef.FullName;
                    /* 
                    if (typeRef.IsNested)
                    {
                        fullNameForNested = fullName.Replace("/", ".");
                    }
                    else
                        fullNameForNested = fullName;
                    */
                }
                return fullName;
            }
        }
        public string Name
        {
            get
            {
                return typeRef.Name;
            }
        }

        public StackObject DefaultObject { get { return default(StackObject); } }
        public int TypeIndex
        {
            get
            {
                if (tIdx < 0)
                    tIdx = appdomain.AllocTypeIndex(this);
                return tIdx;
            }
        }

        public List<IMethod> GetMethods ()
        {
            if ( methods == null )
                InitializeMethods ();
            List<IMethod> res = new List<IMethod> ();
            foreach ( var i in methods )
            {
                foreach ( var j in i.Value )
                    res.Add ( j );
            }

            return res;
        }
        void InitializeInterfaces ()
        {
            interfaceInitialized = true;
            if ( definition != null && definition.HasInterfaces )
            {
                interfaces = new IType [ definition.Interfaces.Count ];
                for ( int i = 0; i < interfaces.Length; i++ )
                {
                    interfaces [ i ] = appdomain.GetType ( definition.Interfaces [ i ].InterfaceType, this, null );
                    //only one clrInterface is valid
                    if ( interfaces [ i ] is CLRType && firstCLRInterface == null )
                    {
                        CrossBindingAdaptor adaptor;
                        if ( appdomain.CrossBindingAdaptors.TryGetValue ( interfaces [ i ].TypeForCLR, out adaptor ) )
                        {
                            interfaces [ i ] = adaptor;
                            firstCLRInterface = adaptor;
                        }
                        else
                            throw new TypeLoadException ( "Cannot find Adaptor for:" + interfaces [ i ].TypeForCLR.ToString () );
                    }
                }
            }
            if ( firstCLRInterface == null && BaseType != null && BaseType is ILType )
                firstCLRInterface = ( ( ILType ) BaseType ).FirstCLRInterface;
        }
        void InitializeBaseType ()
        {
            if ( definition != null && definition.BaseType != null )
            {
                bool specialProcess = false;
                List<int> spIdx = null;
                if ( definition.BaseType.IsGenericInstance )
                {
                    GenericInstanceType git = definition.BaseType as GenericInstanceType;
                    var elementType = appdomain.GetType ( git.ElementType, this, null );
                    if ( elementType is CLRType )
                    {
                        for ( int i = 0; i < git.GenericArguments.Count; i++ )
                        {
                            var ga = git.GenericArguments [ i ];
                            if ( ga == typeRef )
                            {
                                specialProcess = true;
                                if ( spIdx == null )
                                    spIdx = new List<int> ();
                                spIdx.Add ( i );
                            }
                        }
                    }
                }
                if ( specialProcess )
                {
                    //如果泛型参数是自身，则必须要特殊处理，否则会StackOverflow
                    var elementType = appdomain.GetType ( ( ( GenericInstanceType ) definition.BaseType ).ElementType, this, null );
                    foreach ( var i in appdomain.CrossBindingAdaptors )
                    {
                        if ( i.Key.IsGenericType && !i.Key.IsGenericTypeDefinition )
                        {
                            var gd = i.Key.GetGenericTypeDefinition ();
                            if ( gd == elementType.TypeForCLR )
                            {
                                var ga = i.Key.GetGenericArguments ();
                                bool match = true;
                                foreach ( var j in spIdx )
                                {
                                    if ( ga [ j ] != i.Value.AdaptorType )
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if ( match )
                                {
                                    baseType = i.Value;
                                    break;
                                }
                            }
                        }
                    }
                    if ( baseType == null )
                    {
                        throw new TypeLoadException ( "Cannot find Adaptor for:" + definition.BaseType.FullName );
                    }
                }
                else
                {
                    baseType = appdomain.GetType ( definition.BaseType, this, null );
                    if ( baseType is CLRType )
                    {
                        if ( baseType.TypeForCLR == typeof ( Enum ) || baseType.TypeForCLR == typeof ( object ) || baseType.TypeForCLR == typeof ( ValueType ) || baseType.TypeForCLR == typeof ( System.Enum ) )
                        {//都是这样，无所谓
                            baseType = null;
                        }
                        else if ( baseType.TypeForCLR == typeof ( MulticastDelegate ) )
                        {
                            baseType = null;
                            isDelegate = true;
                        }
                        else
                        {
                            CrossBindingAdaptor adaptor;
                            if ( appdomain.CrossBindingAdaptors.TryGetValue ( baseType.TypeForCLR, out adaptor ) )
                            {
                                baseType = adaptor;
                            }
                            else
                                throw new TypeLoadException ( "Cannot find Adaptor for:" + baseType.TypeForCLR.ToString () );
                            //继承了其他系统类型
                            //env.logger.Log_Error("ScriptType:" + Name + " Based On a SystemType:" + BaseType.Name);
                            //HasSysBase = true;
                            //throw new Exception("不得继承系统类型，脚本类型系统和脚本类型系统是隔离的");
                        }
                    }
                }
            }
            var curBase = baseType;
            while ( curBase is ILType )
            {
                curBase = curBase.BaseType;
            }
            firstCLRBaseType = curBase;
            baseTypeInitialized = true;
        }

        public IMethod GetMethod ( string name )
        {
            if ( methods == null )
                InitializeMethods ();
            List<ILMethod> lst;
            if ( methods.TryGetValue ( name, out lst ) )
            {
                return lst [ 0 ];
            }
            return null;
        }

        public IMethod GetMethod ( string name, int paramCount, bool declaredOnly = false )
        {
            if ( methods == null )
                InitializeMethods ();
            List<ILMethod> lst;
            if ( methods.TryGetValue ( name, out lst ) )
            {
                foreach ( var i in lst )
                {
                    if ( i.ParameterCount == paramCount )
                        return i;
                }
            }
            if ( declaredOnly )
                return null;
            else
            {
                //skip clr base type, this doesn't make any sense
                if ( BaseType != null && !( BaseType is CrossBindingAdaptor ) )
                    return BaseType.GetMethod ( name, paramCount, false );
                else
                    return null;
            }
        }

        void InitializeMethods ()
        {
            methods = new Dictionary<string, List<ILMethod>> ();
            constructors = new List<ILMethod> ();
            if ( definition == null )
                return;
            if ( definition.HasCustomAttributes )
            {
                for ( int i = 0; i < definition.CustomAttributes.Count; i++ )
                {
                    int f;
                    if ( definition.CustomAttributes [ i ].GetJITFlags ( AppDomain, out f ) )
                    {
                        this.jitFlags = f;
                        break;
                    }
                }
            }
            foreach ( var i in definition.Methods )
            {
                if ( i.IsConstructor )
                {
                    if ( i.IsStatic )
                        staticConstructor = new ILMethod ( i, this, appdomain, jitFlags );
                    else
                        constructors.Add ( new ILMethod ( i, this, appdomain, jitFlags ) );
                }
                else
                {
                    List<ILMethod> lst;
                    if ( !methods.TryGetValue ( i.Name, out lst ) )
                    {
                        lst = new List<ILMethod> ();
                        methods [ i.Name ] = lst;
                    }
                    var m = new ILMethod ( i, this, appdomain, jitFlags );
                    lst.Add ( m );
                }
            }

            if ( !appdomain.SuppressStaticConstructor && !staticConstructorCalled )
            {
                staticConstructorCalled = true;
                if ( staticConstructor != null && ( !TypeReference.HasGenericParameters || IsGenericInstance ) )
                {
                    appdomain.Invoke ( staticConstructor, null, null );
                }
            }
        }

        public IMethod GetVirtualMethod ( IMethod method )
        {
            IType [] genericArguments = null;
            if ( method.IsGenericInstance )
            {
                if ( method is ILMethod )
                {
                    genericArguments = ( ( ILMethod ) method ).GenericArugmentsArray;
                }
                else
                {
                    genericArguments = ( ( CLRMethod ) method ).GenericArguments;
                }
            }

            var m = GetMethod ( method.Name, method.Parameters, genericArguments, method.ReturnType, true );
            if ( m == null && BaseType != null )
            {
                m = BaseType.GetVirtualMethod ( method );
                if ( m != null )
                    return m;
            }
            if ( m == null && method.DeclearingType.IsInterface )
            {
                if ( method.DeclearingType is ILType )
                {
                    ILType iltype = ( ILType ) method.DeclearingType;
                    m = GetMethod ( string.Format ( "{0}.{1}", iltype.FullNameForNested, method.Name ), method.Parameters, genericArguments, method.ReturnType, true );
                }
                else
                    m = GetMethod ( string.Format ( "{0}.{1}", method.DeclearingType.FullName, method.Name ), method.Parameters, genericArguments, method.ReturnType, true );
            }

            if ( m == null || m.IsGenericInstance == method.IsGenericInstance )
                return m;
            else
                return method;

        }

        public IMethod GetMethod ( string name, List<IType> param, IType [] genericArguments, IType returnType = null, bool declaredOnly = false )
        {
            if ( methods == null )
                InitializeMethods ();
            List<ILMethod> lst;
            IMethod genericMethod = null;
            if ( methods.TryGetValue ( name, out lst ) )
            {
                for ( var idx = 0; idx < lst.Count; idx++ )
                {
                    var i = lst [ idx ];
                    int pCnt = param != null ? param.Count : 0;
                    if ( i.ParameterCount == pCnt )
                    {
                        bool match = true;
                        if ( genericArguments != null && i.GenericParameterCount == genericArguments.Length && genericMethod == null )
                        {
                            genericMethod = CheckGenericParams ( i, param, genericArguments, ref match );
                        }
                        else
                        {
                            match = CheckGenericArguments ( i, genericArguments );
                            if ( !match )
                                continue;
                            for ( int j = 0; j < pCnt; j++ )
                            {
                                if ( param [ j ] != i.Parameters [ j ] )
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if ( match )
                            {
                                match = returnType == null || i.ReturnType == returnType;
                            }
                            if ( match )
                                return i;
                        }
                    }
                }
            }
            if ( genericArguments != null && genericMethod != null )
            {
                var m = genericMethod.MakeGenericMethod ( genericArguments );
                lst.Add ( ( ILMethod ) m );
                return m;
            }
            if ( declaredOnly )
                return null;
            else
            {
                if ( BaseType != null )
                    return BaseType.GetMethod ( name, param, genericArguments, returnType, false );
                else
                    return null;
            }
        }

        bool CheckGenericArguments ( ILMethod i, IType [] genericArguments )
        {
            if ( genericArguments == null )
            {
                return i.GenericArguments == null;
            }
            else
            {
                if ( i.GenericArguments == null )
                    return false;
                else if ( i.GenericArguments.Length != genericArguments.Length )
                    return false;
                if ( i.GenericArguments.Length == genericArguments.Length )
                {
                    for ( int j = 0; j < genericArguments.Length; j++ )
                    {
                        if ( i.GenericArguments [ j ].Value != genericArguments [ j ] )
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }
        }

        bool IsGenericArgumentMatch ( IType p, IType p2, IType [] genericArguments )
        {
            bool found = false;
            foreach ( var a in genericArguments )
            {
                if ( a == p2 )
                {
                    found = true;
                    break;
                }
            }
            if ( !found )
            {
                return false;
            }
            else
                return true;
        }

        ILMethod CheckGenericParams ( ILMethod i, List<IType> param, IType [] genericArguments, ref bool match )
        {
            ILMethod genericMethod = null;
            if ( param != null )
            {
                for ( int j = 0; j < param.Count; j++ )
                {
                    var p = i.Parameters [ j ];
                    if ( p.IsGenericParameter )
                    {
                        if ( IsGenericArgumentMatch ( p, param [ j ], genericArguments ) )
                            continue;
                        else
                        {
                            match = false;
                            break;
                        }
                    }
                    if ( p.IsByRef )
                        p = p.ElementType;
                    if ( p.IsArray )
                        p = p.ElementType;

                    var p2 = param [ j ];
                    if ( p2.IsByRef )
                        p2 = p2.ElementType;
                    if ( p2.IsArray )
                        p2 = p2.ElementType;
                    if ( p.IsGenericParameter )
                    {
                        if ( i.Parameters [ j ].IsByRef == param [ j ].IsByRef && i.Parameters [ j ].IsArray == param [ j ].IsArray && IsGenericArgumentMatch ( p, p2, genericArguments ) )
                            continue;
                        else
                        {
                            match = false;
                            break;
                        }
                    }
                    if ( p.HasGenericParameter )
                    {
                        if ( p.Name != p2.Name )
                        {
                            match = false;
                            break;
                        }
                        //TODO should match the generic parameters;
                        continue;
                    }


                    if ( p2 != p )
                    {
                        match = false;
                        break;
                    }
                }
            }
            if ( match )
            {
                genericMethod = i;
            }
            return genericMethod;
        }

        public List<ILMethod> GetConstructors ()
        {
            if ( constructors == null )
                InitializeMethods ();
            return constructors;
        }

        public IMethod GetStaticConstroctor ()
        {
            if ( constructors == null )
                InitializeMethods ();
            return staticConstructor;
        }

        public IMethod GetConstructor ( int paramCnt )
        {
            if ( constructors == null )
                InitializeMethods ();
            foreach ( var i in constructors )
            {
                if ( i.ParameterCount == paramCnt )
                {
                    return i;
                }
            }
            return null;
        }
        public IMethod GetConstructor(List<IType> param)
        {
            return GetConstructor(param, true);
        }
        public IMethod GetConstructor(List<IType> param, bool exactMatch = true)
        {
            if (constructors == null)
                InitializeMethods();
            foreach (var i in constructors)
            {
                if (i.ParameterCount == param.Count)
                {
                    bool match = true;

                    for (int j = 0; j < param.Count; j++)
                    {
                        if ((exactMatch && param[j] != i.Parameters[j]) || !i.Parameters[j].CanAssignTo(param[j]))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return i;
                }
            }
            return null;
        }

        public int GetFieldIndex ( object token )
        {
            if ( fieldMapping == null )
                InitializeFields ();
            int idx;
            int hashCode = token.GetHashCode ();
            if ( fieldTokenMapping.TryGetValue ( hashCode, out idx ) )
                return idx;
            FieldReference f = token as FieldReference;
            if ( staticFieldMapping != null && staticFieldMapping.TryGetValue ( f.Name, out idx ) )
            {
                fieldTokenMapping [ hashCode ] = idx;
                return idx;
            }
            if ( fieldMapping.TryGetValue ( f.Name, out idx ) )
            {
                fieldTokenMapping [ hashCode ] = idx;
                return idx;
            }

            return -1;
        }

        public IType GetField ( string name, out int fieldIdx )
        {
            if ( fieldMapping == null )
                InitializeFields ();
            if ( fieldMapping.TryGetValue ( name, out fieldIdx ) )
            {
                return fieldTypes [ fieldIdx - FieldStartIndex ];
            }
            else if ( BaseType != null && BaseType is ILType )
            {
                return ( ( ILType ) BaseType ).GetField ( name, out fieldIdx );
            }
            else
                return null;
        }

        public IType GetField ( int fieldIdx, out FieldDefinition fd )
        {
            if ( fieldMapping == null )
                InitializeFields ();
            if ( fieldIdx < FieldStartIndex )
                return ( ( ILType ) BaseType ).GetField ( fieldIdx, out fd );
            else
            {
                fd = fieldDefinitions [ fieldIdx - FieldStartIndex ];
                return fieldTypes [ fieldIdx - FieldStartIndex ];
            }
        }

        void InitializeFields ()
        {
            fieldMapping = new Dictionary<string, int> ();
            if ( definition == null )
            {
                fieldTypes = new IType [ 0 ];
                fieldDefinitions = new FieldDefinition [ 0 ];
                return;
            }
            fieldTypes = new IType [ definition.Fields.Count ];
            fieldDefinitions = new FieldDefinition [ definition.Fields.Count ];
            var fields = definition.Fields;
            int idx = FieldStartIndex;
            int idxStatic = 0;
            for ( int i = 0; i < fields.Count; i++ )
            {
                var field = fields [ i ];
                if ( field.IsStatic )
                {
                    //It makes no sence to initialize
                    if ( !TypeReference.HasGenericParameters || IsGenericInstance )
                    {
                        if ( staticFieldTypes == null )
                        {
                            staticFieldTypes = new IType [ definition.Fields.Count ];
                            staticFieldDefinitions = new FieldDefinition [ definition.Fields.Count ];
                            staticFieldMapping = new Dictionary<string, int> ();
                        }
                        staticFieldMapping [ field.Name ] = idxStatic;
                        staticFieldDefinitions [ idxStatic ] = field;
                        if ( field.FieldType.IsGenericParameter )
                        {
                            staticFieldTypes [ idxStatic ] = FindGenericArgument ( field.FieldType.Name );
                        }
                        else
                            staticFieldTypes [ idxStatic ] = appdomain.GetType ( field.FieldType, this, null );
                        idxStatic++;
                    }
                }
                else
                {
                    fieldMapping [ field.Name ] = idx;
                    fieldDefinitions [ idx - FieldStartIndex ] = field;
                    if ( field.FieldType.IsGenericParameter )
                    {
                        fieldTypes [ idx - FieldStartIndex ] = FindGenericArgument ( field.FieldType.Name );
                    }
                    else
                        fieldTypes [ idx - FieldStartIndex ] = appdomain.GetType ( field.FieldType, this, null );
                    if ( IsEnum )
                    {
                        enumType = fieldTypes [ idx - FieldStartIndex ];
                    }
                    idx++;
                }
            }
            Array.Resize ( ref fieldTypes, idx - FieldStartIndex );
            Array.Resize ( ref fieldDefinitions, idx - FieldStartIndex );

            if ( staticFieldTypes != null )
            {
                Array.Resize ( ref staticFieldTypes, idxStatic );
                Array.Resize ( ref staticFieldDefinitions, idxStatic );
                //staticInstance = new ILTypeStaticInstance(this);
            }
        }

        public IType FindGenericArgument ( string key )
        {
            var o = this.Generic ( key );
            if ( o == null && definition.GenericParameters != null )
            {
                for ( int i = 0; i < definition.GenericParameters.Count; i++ )
                {
                    if ( definition.GenericParameters [ i ].Name == key )
                    {
                        return this.Generic ( "!" + i );
                    }
                }
            }
            return o;
        }

        private IType Generic ( string key )
        {
            if ( this.genericArguments != null )
            {
                for ( int i = 0; i < this.genericArguments.Length; i++ )
                {
                    if ( this.genericArguments [ i ].Key == key )
                    {
                        return this.genericArguments [ i ].Value;
                    }
                }
            }

            return null;
        }

        public bool CanAssignTo ( IType type )
        {
            bool res = false;
            if ( this == type )
            {
                return true;
            }

            if ( IsEnum )
            {
                if ( type.TypeForCLR == typeof ( Enum ) )
                    return true;
            }
            if ( BaseType != null )
            {
                res = BaseType.CanAssignTo ( type );

                if ( res ) return true;
            }

            if ( Implements != null )
            {
                for ( int i = 0; i < interfaces.Length; i++ )
                {
                    var im = interfaces [ i ];
                    res = im.CanAssignTo ( type );
                    if ( res )
                        return true;
                }
            }
            return res;
        }

        public ILTypeInstance Instantiate ( bool callDefaultConstructor = true )
        {
            var res = new ILTypeInstance ( this );
            if ( callDefaultConstructor )
            {
                var m = GetConstructor ( CLR.Utils.Extensions.EmptyParamList );
                if ( m != null )
                {
                    appdomain.Invoke ( m, res, null );
                }
            }
            return res;
        }

        public ILTypeInstance Instantiate(object[] args)
        {
            var res = new ILTypeInstance(this);
            var argsTypes = new List<IType>(args.Length);
            foreach (var o in args)
            {
                if (o is ILTypeInstance)
                {
                    argsTypes.Add(((ILTypeInstance)o).Type);
                }
                else
                {
                    argsTypes.Add(appdomain.GetType(o.GetType()));
                }
            }
            var m = GetConstructor(argsTypes, false);
            if (m != null)
            {
                appdomain.Invoke(m, res, args);
            }

            return res;
        }

        public IType MakeGenericInstance ( KeyValuePair<string, IType> [] genericArguments )
        {
            if ( genericInstances == null )
                genericInstances = new List<ILType> ();
            foreach ( var i in genericInstances )
            {
                bool match = true;
                for ( int j = 0; j < genericArguments.Length; j++ )
                {
                    if ( i.genericArguments [ j ].Value != genericArguments [ j ].Value )
                    {
                        match = false;
                        break;
                    }
                }
                if ( match )
                    return i;
            }
            var res = new ILType ( definition, appdomain );
            res.genericDefinition = this;
            res.genericArguments = genericArguments;

            genericInstances.Add ( res );
            return res;
        }

        public IType MakeByRefType ()
        {
            if ( byRefType == null )
            {
                var def = new ByReferenceType ( typeRef );
                byRefType = new ILType ( def, appdomain );
                ( ( ILType ) byRefType ).elementType = this;
                ( ( ILType ) byRefType ).byRefCLRType = this.TypeForCLR.MakeByRefType ();
            }
            return byRefType;
        }

        public IType MakeArrayType ( int rank )
        {
            if ( arrayTypes == null )
                arrayTypes = new Dictionary<int, IType> ();
            IType atype;
            if ( !arrayTypes.TryGetValue ( rank, out atype ) )
            {
                var def = new ArrayType ( typeRef, rank );
                atype = new ILType ( def, appdomain );
                ( ( ILType ) atype ).IsArray = true;
                ( ( ILType ) atype ).elementType = this;
                ( ( ILType ) atype ).arrayCLRType = rank > 1 ? this.TypeForCLR.MakeArrayType ( rank ) : this.TypeForCLR.MakeArrayType ();
                arrayTypes [ rank ] = atype;
            }
            return atype;
        }

        public IType ResolveGenericType ( IType contextType )
        {
            var ga = contextType.GenericArguments;
            if ( definition == null )
                return null;
            IType [] kv = new IType [ definition.GenericParameters.Count ];
            for ( int i = 0; i < kv.Length; i++ )
            {
                var gp = definition.GenericParameters [ i ];
                string name = gp.Name;
                foreach ( var j in ga )
                {
                    if ( j.Key == name )
                    {
                        kv [ i ] = j.Value;
                        break;
                    }
                }
            }

            foreach ( var i in genericInstances )
            {
                bool match = true;
                for ( int j = 0; j < kv.Length; j++ )
                {
                    if ( i.genericArguments [ j ].Value != kv [ j ] )
                    {
                        match = false;
                        break;
                    }
                }
                if ( match )
                    return i;
            }

            return null;
        }

        public int GetStaticFieldSizeInMemory ( HashSet<object> traversed )
        {
            return staticInstance != null ? staticInstance.GetSizeInMemory ( traversed ) : 0;
        }

        public unsafe int GetMethodBodySizeInMemory ()
        {
            int size = 0;
            if ( methods != null )
            {
                foreach ( var i in methods )
                {
                    foreach ( var j in i.Value )
                    {
                        if ( j.HasBody )
                        {
                            size += j.Body.Length * sizeof ( Runtime.Intepreter.OpCodes.OpCode );
                        }
                    }
                }
            }
            return size;
        }

        public void GetValueTypeSize ( out int fieldCout, out int managedCount )
        {
            if ( !valuetypeSizeCalculated )
            {
                valuetypeFieldCount = FieldTypes.Length + 1;
                valuetypeManagedCount = 0;
                for ( int i = 0; i < FieldTypes.Length; i++ )
                {
                    var ft = FieldTypes [ i ];
                    if ( ft.IsValueType )
                    {
                        if ( !ft.IsPrimitive && !ft.IsEnum )
                        {
                            if ( ft is ILType || ( ( CLRType ) ft ).ValueTypeBinder != null )
                            {
                                int fSize, fmCnt;
                                ft.GetValueTypeSize ( out fSize, out fmCnt );
                                valuetypeFieldCount += fSize;
                                valuetypeManagedCount += fmCnt;
                            }
                            else
                            {
                                valuetypeManagedCount++;
                            }
                        }
                    }
                    else
                    {
                        valuetypeManagedCount++;
                    }
                }
                if ( BaseType != null && BaseType is ILType )
                {
                    int fSize, fmCnt;
                    BaseType.GetValueTypeSize ( out fSize, out fmCnt );
                    valuetypeFieldCount += fSize - 1;//no header for base type fields
                    valuetypeManagedCount += fmCnt;
                }
                valuetypeSizeCalculated = true;
            }
            fieldCout = valuetypeFieldCount;
            managedCount = valuetypeManagedCount;
        }

        public override int GetHashCode ()
        {
            if ( hashCode == -1 )
                hashCode = System.Threading.Interlocked.Add ( ref instance_id, 1 );
            return hashCode;
        }

        public override string ToString ()
        {
            return FullName;
        }
    }
}
