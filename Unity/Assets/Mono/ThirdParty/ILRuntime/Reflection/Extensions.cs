using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Reflection
{
    static class Extensions
    {
        public static object CreateInstance(this CustomAttribute attribute, IType at, Runtime.Enviorment.AppDomain appdomain)
        {
            object ins;
            List<IType> param = null;
            if (at is ILType)
            {
                var it = (ILType)at;
                if (!attribute.HasConstructorArguments)
                    ins = it.Instantiate(true);
                else
                {
                    ins = it.Instantiate(false);
                    if (param == null)
                        param = new List<IType>();
                    param.Clear();
                    object[] p = new object[attribute.ConstructorArguments.Count];
                    for (int j = 0; j < attribute.ConstructorArguments.Count; j++)
                    {
                        var ca = attribute.ConstructorArguments[j];
                        param.Add(appdomain.GetType(ca.Type, null, null));
                        p[j] = ca.Value;
                    }
                    var ctor = it.GetConstructor(param);
                    appdomain.Invoke(ctor, ins, p);
                }

                if (attribute.HasProperties)
                {
                    object[] p = new object[1];
                    foreach (var j in attribute.Properties)
                    {
                        p[0] = j.Argument.Value;
                        var setter = it.GetMethod("set_" + j.Name, 1);
                        appdomain.Invoke(setter, ins, p);
                    }
                }
                if(attribute.HasFields)
                {
                    foreach (var j in attribute.Fields)
                    {
                        var field = it.GetField(j.Name, out int index);
                        if (field != null)
                            ((ILRuntime.Runtime.Intepreter.ILTypeInstance)ins)[index] = j.Argument.Value;
                    }
                }
                ins = ((ILRuntime.Runtime.Intepreter.ILTypeInstance)ins).CLRInstance;
            }
            else
            {
                param = new List<IType>();
                object[] p = null;
                if (attribute.HasConstructorArguments)
                {
                    p = new object[attribute.ConstructorArguments.Count];
                    for (int j = 0; j < attribute.ConstructorArguments.Count; j++)
                    {
                        var ca = attribute.ConstructorArguments[j];
                        param.Add(appdomain.GetType(ca.Type, null, null));
                        p[j] = ca.Value;
                    }
                }
                ins = ((CLRMethod)at.GetConstructor(param)).ConstructorInfo.Invoke(p);
                if (attribute.HasProperties)
                {
                    foreach (var j in attribute.Properties)
                    {
                        var prop = at.TypeForCLR.GetProperty(j.Name);
                        prop.SetValue(ins, j.Argument.Value, null);
                    }
                }
                if(attribute.HasFields)
                {
                    foreach(var j in attribute.Fields)
                    {
                        var field = at.TypeForCLR.GetField(j.Name);
                        field.SetValue(ins, j.Argument.Value);
                    }
                }
            }

            return ins;
        }
    }
}
