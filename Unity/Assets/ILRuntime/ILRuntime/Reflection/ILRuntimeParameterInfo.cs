using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Reflection
{
    public class ILRuntimeParameterInfo : ParameterInfo
    {
        IType type;

        public ILRuntimeParameterInfo(IType type)
        {
            this.type = type;
        }
        public override Type ParameterType
        {
            get
            {
                return type.ReflectionType;
            }
        }
    }
}
