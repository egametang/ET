using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReversePInvokeWrapperGenerationAttribute : Attribute
    {
        public int ReserveWrapperCount { get; }

        public ReversePInvokeWrapperGenerationAttribute(int reserveWrapperCount)
        {
            ReserveWrapperCount = reserveWrapperCount;
        }
    }
}
