using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;

namespace ILRuntime.CLR.TypeSystem
{
    public interface IType
    {
        bool IsGenericInstance { get; }
        KeyValuePair<string, IType>[] GenericArguments { get; }
        Type TypeForCLR { get; }
        Type ReflectionType { get; }

        IType BaseType { get; }

        IType[] Implements { get; }

        IType ByRefType { get; }

        IType ArrayType { get; }

        string FullName { get; }

        string Name { get; }

        bool IsArray { get; }

        int ArrayRank { get; }

        bool IsValueType { get; }

        bool IsDelegate { get; }

        bool IsPrimitive { get; }

        bool IsByRef { get; }

        bool IsInterface { get; }

        IType ElementType { get; }

        bool HasGenericParameter { get; }

        bool IsGenericParameter { get; }

        ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get; }

        /// <summary>
        /// Get a specified Method in this type
        /// </summary>
        /// <param name="name">Name of the Type</param>
        /// <param name="paramCount">Parameter count</param>
        /// <param name="declaredOnly">True to search the methods decleared in this type only, false to search base types.</param>
        /// <returns></returns>
        IMethod GetMethod(string name, int paramCount, bool declaredOnly = false);
        /// <summary>
        ///  Get a specified Method in this type
        /// </summary>
        /// <param name="name">Name of the Type</param>
        /// <param name="param">List of parameter's types</param>
        /// <param name="genericArguments">List of Generic Arguments</param>
        /// <param name="returnType">Return Type</param>
        /// <param name="declaredOnly">True to search the methods decleared in this type only, false to search base types.</param>
        /// <returns></returns>
        IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null, bool declaredOnly = false);
        IMethod GetVirtualMethod(IMethod method);

        List<IMethod> GetMethods();

        int GetFieldIndex(object token);

        IMethod GetConstructor(List<IType> param);

        bool CanAssignTo(IType type);

        IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments);

        IType MakeByRefType();

        IType MakeArrayType(int rank);
        IType FindGenericArgument(string key);

        IType ResolveGenericType(IType contextType);
    }
}
