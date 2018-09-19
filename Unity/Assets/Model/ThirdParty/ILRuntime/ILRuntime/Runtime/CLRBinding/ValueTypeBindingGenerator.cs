using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.CLRBinding
{
    static class ValueTypeBindingGenerator
    {
        internal static string GenerateValueTypeRegisterCode(this Type type, string typeClsName)
        {
            StringBuilder sb = new StringBuilder();

            if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
            {
                sb.AppendLine("            app.RegisterCLRMemberwiseClone(type, PerformMemberwiseClone);");
            }
            return sb.ToString();
        }

        internal static string GenerateCloneWraperCode(this Type type, FieldInfo[] fields, string typeClsName)
        {
            if (!type.IsValueType || type.IsPrimitive) return string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("        static object PerformMemberwiseClone(ref object o)");
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var ins = new {0}();", typeClsName));
            sb.AppendLine(string.Format("            ins = ({0})o;", typeClsName));
            sb.AppendLine("            return ins;");
            sb.AppendLine("        }");
            return sb.ToString();

        }
    }
}
