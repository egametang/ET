using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Huatuo.Generators
{
    internal abstract class PlatformAdaptorBase : IPlatformAdaptor
    {
        public abstract TypeInfo Create(ParameterInfo param, bool returnValue);

        protected abstract void GenMethod(MethodBridgeSig method, List<string> lines);

        public abstract IEnumerable<MethodBridgeSig> GetPreserveMethods();

        private static Dictionary<Type, int> _typeSizeCache = new Dictionary<Type, int>();

        public static int ComputeSizeOf(Type t)
        {
            if (_typeSizeCache.TryGetValue(t, out var size))
            {
                return size;
            }
            // all this just to invoke one opcode with no arguments!
            var method = new DynamicMethod("ComputeSizeOfImpl", typeof(int), Type.EmptyTypes, typeof(PlatformAdaptorBase), false);
            var gen = method.GetILGenerator();
            gen.Emit(OpCodes.Sizeof, t);
            gen.Emit(OpCodes.Ret);
            size = ((Func<int>)method.CreateDelegate(typeof(Func<int>)))();
            _typeSizeCache.Add(t, size);
            return size;
        }

        protected void GenCallStub(List<MethodBridgeSig> methods, List<string> lines)
        {
            lines.Add($@"
NativeCallMethod huatuo::interpreter::g_callStub[] = 
{{
");

            foreach (var method in methods)
            {
                lines.Add($"\t{{\"{method.CreateInvokeSigName()}\", (Il2CppMethodPointer)__Native2ManagedCall_{method.CreateInvokeSigName()}, (Il2CppMethodPointer)__Native2ManagedCall_AdjustorThunk_{method.CreateCallSigName()}, __Managed2NativeCall_{method.CreateInvokeSigName()}}},");
            }

            lines.Add($"\t{{nullptr, nullptr}},");
            lines.Add("};");
        }

        protected void GenInvokeStub(List<MethodBridgeSig> methods, List<string> lines)
        {
            lines.Add($@"
NativeInvokeMethod huatuo::interpreter::g_invokeStub[] = 
{{
");

            foreach (var method in methods)
            {
                lines.Add($"\t{{\"{method.CreateInvokeSigName()}\", __Invoke_instance_{method.CreateInvokeSigName()}, __Invoke_static_{method.CreateInvokeSigName()}}},");
            }

            lines.Add($"\t{{nullptr, nullptr, nullptr}},");
            lines.Add("};");
        }

        public void Generate(List<MethodBridgeSig> methods, List<string> outputLines)
        {
            foreach (var method in methods)
            {
                GenMethod(method, outputLines);
            }
            GenCallStub(methods, outputLines);
            GenInvokeStub(methods, outputLines);
        }
    }
}
