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
                }

                idx++;
            }
            return sb.ToString();
        }

        internal static string GenerateFieldWraperCode(this Type type, FieldInfo[] fields, string typeClsName, HashSet<FieldInfo> excludes)
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

                if (!i.IsInitOnly && !i.IsLiteral)
                {
                    sb.AppendLine(string.Format("        static void set_{0}_{1}(ref object o, object v)", i.Name, idx));
                    sb.AppendLine("        {");
                    string clsName, realClsName;
                    bool isByRef;
                    i.FieldType.GetClassName(out clsName, out realClsName, out isByRef);
                    if (i.IsStatic)
                    {
                        sb.AppendLine(string.Format("            {0}.{1} = ({2})v;", typeClsName, i.Name, realClsName));
                    }
                    else
                    {
                        if (CheckCanPinn(type))
                        {
                            sb.AppendLine("            var h = GCHandle.Alloc(o, GCHandleType.Pinned);");
                            sb.AppendLine(string.Format("            {0}* p = ({0} *)(void *)h.AddrOfPinnedObject();", typeClsName));
                            sb.AppendLine(string.Format("            p->{0} = ({1})v;", i.Name, realClsName));
                            sb.AppendLine("            h.Free();");
                        }
                        else
                        {
                            sb.AppendLine(string.Format("            (({0})o).{1} = ({2})v;", typeClsName, i.Name, realClsName));
                        }
                    }
                    sb.AppendLine("        }");
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
