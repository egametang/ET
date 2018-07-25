using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.CLRBinding
{
    static class CommonBindingGenerator
    {
        internal static string GenerateMiscRegisterCode(this Type type, string typeClsName, bool defaultCtor, bool newArr)
        {
            StringBuilder sb = new StringBuilder();

            if (defaultCtor && !type.IsPrimitive && !type.IsAbstract)
            {
                var constructorFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var hasDefaultConstructor = type.GetConstructor(constructorFlags, null, new Type[0], null) != null;

                if (hasDefaultConstructor || type.IsValueType)
                {
                    sb.AppendLine(string.Format("            app.RegisterCLRCreateDefaultInstance(type, () => new {0}());", typeClsName));
                }
            }

            if (newArr)
            {
                if (!type.IsAbstract || !type.IsSealed)
                {
                    if (type.IsArray)
                    {
                        Type elementType = type;
                        int arrCnt = 0;
                        while (elementType.IsArray)
                        {
                            elementType = elementType.GetElementType();
                            arrCnt++;
                        }
                        string elem, clsName;
                        bool isByRef;
                        elementType.GetClassName(out clsName, out elem, out isByRef);
                        string trail = "";
                        for (int i = 0; i < arrCnt; i++)
                            trail += "[]";
                        sb.AppendLine(string.Format("            app.RegisterCLRCreateArrayInstance(type, s => new {0}[s]{1});", elem, trail));
                    }
                    else
                        sb.AppendLine(string.Format("            app.RegisterCLRCreateArrayInstance(type, s => new {0}[s]);", typeClsName));
                }
            }

            return sb.ToString();
        }
        internal static string GenerateCommonCode(this Type type, string typeClsName)
        {
            if (!type.IsValueType)
                return "";
            StringBuilder sb = new StringBuilder();
            if (type.IsPrimitive)
            {
                sb.AppendLine(string.Format("        static {0} GetInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack)", typeClsName));
                sb.AppendLine("        {");
                if (type.IsPrimitive || type.IsValueType)
                    sb.AppendLine("            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                sb.AppendLine(string.Format("            {0} instance_of_this_method;", typeClsName));
                sb.Append(@"            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.FieldReference:
                    {
                        var instance_of_fieldReference = __mStack[ptr_of_this_method->Value];
                        if(instance_of_fieldReference is ILTypeInstance)
                        {
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(")typeof(");
                sb.Append(typeClsName);
                
                sb.Append(").CheckCLRTypes(((ILTypeInstance)instance_of_fieldReference)[ptr_of_this_method->ValueLow])");
                sb.Append(";");
                sb.Append(@"
                        }
                        else
                        {
                            var t = __domain.GetType(instance_of_fieldReference.GetType()) as CLRType;
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(")t.GetFieldValue(ptr_of_this_method->ValueLow, instance_of_fieldReference);");
                sb.Append(@"
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(")typeof(");
                sb.Append(typeClsName);
                sb.Append(").CheckCLRTypes(((ILType)t).StaticInstance[ptr_of_this_method->ValueLow])");
                sb.Append(";");
                sb.Append(@"
                        }
                        else
                        {
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(@")((CLRType)t).GetFieldValue(ptr_of_this_method->ValueLow, null);
                        }
                    }
                    break;
                case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                sb.Append(typeClsName);
                sb.AppendLine(@"[];
                        instance_of_this_method = instance_of_arrayReference[ptr_of_this_method->ValueLow];                        
                    }
                    break;
                default:");
                sb.AppendLine(string.Format("                    instance_of_this_method = {0};", type.GetRetrieveValueCode(typeClsName)));
                sb.AppendLine(@"                    break;
            }
            return instance_of_this_method;");
                sb.AppendLine("        }");
            }
            if (!type.IsPrimitive && !type.IsAbstract)
            {
                sb.AppendLine(string.Format("        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref {0} instance_of_this_method)", typeClsName));
                sb.AppendLine("        {");
                sb.AppendLine(@"            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;");
                sb.Append(@"                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method");
                sb.Append(@";
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method");
                sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method");
                sb.Append(@";
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method");
                sb.Append(@");
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                sb.Append(typeClsName);
                sb.AppendLine(@"[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }");
                sb.AppendLine(@"        }");
            }
            return sb.ToString();
        }
    }
}
