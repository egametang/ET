using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
namespace ILRuntime.CLR.Method
{
    public interface IMethod
    {
        string Name { get; }
        int ParameterCount { get; }

        bool HasThis { get; }

        IType DeclearingType { get; }

        IType ReturnType { get; }
        List<IType> Parameters { get; }

        int GenericParameterCount { get; }

        bool IsGenericInstance { get; }

        bool IsConstructor { get; }

        bool IsDelegateInvoke { get; }

        bool IsStatic { get; }

        IMethod MakeGenericMethod(IType[] genericArguments);


        bool IsExtend { get; }
    }
}
