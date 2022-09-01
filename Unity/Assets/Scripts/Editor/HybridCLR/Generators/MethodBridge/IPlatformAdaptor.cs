using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Generators.MethodBridge
{
    public interface IPlatformAdaptor
    {
        bool IsArch32 { get; }

        TypeInfo CreateTypeInfo(Type type, bool returnValue);

        void GenerateManaged2NativeMethod(MethodBridgeSig method, List<string> outputLines);

        void GenerateManaged2NativeStub(List<MethodBridgeSig> methods, List<string> lines);

        void GenerateNative2ManagedMethod(MethodBridgeSig method, List<string> outputLines);

        void GenerateNative2ManagedStub(List<MethodBridgeSig> methods, List<string> lines);

        void GenerateAdjustThunkMethod(MethodBridgeSig method, List<string> outputLines);

        void GenerateAdjustThunkStub(List<MethodBridgeSig> methods, List<string> lines);
    }
}
