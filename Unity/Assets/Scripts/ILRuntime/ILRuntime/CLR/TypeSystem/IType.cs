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

        bool IsValueType { get; }

        bool IsDelegate { get; }

        bool HasGenericParameter { get; }

        ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get; }

        IMethod GetMethod(string name, int paramCount);

        IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null);
        IMethod GetVirtualMethod(IMethod method);

        List<IMethod> GetMethods();

        int GetFieldIndex(object token);

        IMethod GetConstructor(List<IType> param);

        bool CanAssignTo(IType type);

        IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments);

        IType MakeByRefType();

        IType MakeArrayType();
        IType FindGenericArgument(string key);

        IType ResolveGenericType(IType contextType);
    }
}
