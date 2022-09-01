using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Generators.MethodBridge
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SignatureProviderAttribute : Attribute
    {
        public List<PlatformABI> Platforms { get; }

        public SignatureProviderAttribute(params PlatformABI[] platforms)
        {
            Platforms = new List<PlatformABI>(platforms);
        }
    }
}
