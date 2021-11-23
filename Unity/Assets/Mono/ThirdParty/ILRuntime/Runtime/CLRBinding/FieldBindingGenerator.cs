using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.CLRBinding
{
    static class FieldBindingGenerator
    {
        internal static string GenerateFieldRegisterCode(this Type type, FieldInfo[] fields, HashSet<FieldInfo> excludes)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var i in fields)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipField(i))
                    continue;
                if (i.IsSpecialName)
                    continue;

                sb.AppendLine(string.Format("            field = type.GetField(\"{0}\", flag);", i.Name));
                sb.AppendLine(string.Format("            app.RegisterCLRFieldGetter(field, get_{0}_{1});", i.Name, idx));
                if (!i.IsInitOnly && !i.IsLiteral)
                {
                    sb.AppendLine(string.Format("            app.RegisterCLRFieldSetter(field, set_{0}_{1});", i.Name, idx));
                    sb.AppendLine(string.Format("            app.RegisterCLRFieldBinding(field, CopyToStack_{0}_{1}, AssignFromStack_{0}_{1});", i.Name, idx));
                }
                else
                {
                    sb.AppendLine(string.Format("            app.RegisterCLRFieldBinding(field, CopyToStack_{0}_{1}, null);", i.Name, idx));
                }

                idx++;
            }
            return sb.ToString();
        }

        internal static string GenerateFieldWraperCode(this Type type, FieldInfo[] fields, string typeClsName, HashSet<FieldInfo> excludes, List<Type> valueTypeBinders, Enviorment.AppDomain domain)
        {
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            foreach (var i in fields)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipField(i))
                    continue;
                sb.AppendLine(string.Format("        static object get_{0}_{1}(ref object o)", i.Name, idx));
                sb.AppendLine("        {");
                if (i.IsStatic)
                {
                    sb.AppendLine(string.Format("            return {0}.{1};", typeClsName, i.Name));
                }
                else
                {
                    sb.AppendLine(string.Format("            return (({0})o).{1};", typeClsName, i.Name));
                }
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine(string.Format("        static StackObject* CopyToStack_{0}_{1}(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)", i.Name, idx));
                sb.AppendLine("        {");
                if (i.IsStatic)
                {
                    sb.AppendLine(string.Format("            var result_of_this_method = {0}.{1};", typeClsName, i.Name));
                }
                else
                {
                    sb.AppendLine(string.Format("            var result_of_this_method = (({0})o).{1};", typeClsName, i.Name));
                }
                string clsName, realClsName;
                bool isByRef;
                i.FieldType.GetClassName(out clsName, out realClsName, out isByRef);

                if (i.FieldType.IsValueType && !i.FieldType.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(i.FieldType))
                {
                    
                    sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                    sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);", clsName));
                    sb.AppendLine("                return __ret + 1;");

                    sb.AppendLine("            } else {");

                    i.FieldType.GetReturnValueCode(sb, domain);

                    sb.AppendLine("            }");
                }
                else
                {
                    i.FieldType.GetReturnValueCode(sb, domain);
                }
                sb.AppendLine("        }");
                sb.AppendLine();

                if (!i.IsInitOnly && !i.IsLiteral)
                {
                    sb.AppendLine(string.Format("        static void set_{0}_{1}(ref object o, object v)", i.Name, idx));
                    sb.AppendLine("        {");
                    if (i.IsStatic)
                    {
                        sb.AppendLine(string.Format("            {0}.{1} = ({2})v;", typeClsName, i.Name, realClsName));
                    }
                    else
                    {
                        if (type.IsValueType)
                        {
                            sb.AppendLine(string.Format("            {0} ins =({0})o;", typeClsName));
                            sb.AppendLine(string.Format("            ins.{0} = ({1})v;", i.Name, realClsName));
                            sb.AppendLine("            o = ins;");
                        }
                        else
                            sb.AppendLine(string.Format("            (({0})o).{1} = ({2})v;", typeClsName, i.Name, realClsName));
                    }
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine(string.Format("        static StackObject* AssignFromStack_{0}_{1}(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)", i.Name, idx));
                    sb.AppendLine("        {");
                    sb.AppendLine("            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;");
                    i.FieldType.AppendArgumentCode(sb, 0, i.Name, valueTypeBinders, false, false, false);
                    if (i.IsStatic)
                    {
                        sb.AppendLine(string.Format("            {0}.{1} = @{1};", typeClsName, i.Name));
                    }
                    else
                    {
                        if (type.IsValueType)
                        {
                            sb.AppendLine(string.Format("            {0} ins =({0})o;", typeClsName));
                            sb.AppendLine(string.Format("            ins.{0} = @{0};", i.Name));
                            sb.AppendLine("            o = ins;");
                        }
                        else
                        {
                            sb.AppendLine(string.Format("            (({0})o).{1} = @{1};", typeClsName, i.Name));
                        }
                    }
                    sb.AppendLine("            return ptr_of_this_method;");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
                idx++;
            }

            return sb.ToString();
        }

        internal static bool CheckCanPinn(this Type type)
        {
            if (type.IsValueType)
            {
                FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                bool res = true;
                foreach(var i in fi)
                {
                    if(!i.FieldType.IsPrimitive)
                    {
                        res = false;
                        break;
                    }
                }

                return res;
            }
            else
                return false;
        }
    }
}
