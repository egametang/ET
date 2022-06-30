using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Huatuo.Generators
{
    public interface IPlatformAdaptor
    {
        TypeInfo Create(ParameterInfo param, bool returnValue);

        IEnumerable<MethodBridgeSig> GetPreserveMethods();

        void Generate(List<MethodBridgeSig> methods, List<string> outputLines);
    }
}
