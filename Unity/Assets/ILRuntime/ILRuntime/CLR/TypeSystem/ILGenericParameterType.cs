using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.CLR.TypeSystem
{
    class ILGenericParameterType : IType
    {
        string name;
        ILGenericParameterType arrayType;
        public ILGenericParameterType(string name)
        {
            this.name = name;
        }
        public bool IsGenericInstance
        {
            get { return false; }
        }

        public KeyValuePair<string, IType>[] GenericArguments
        {
            get { return null; }
        }
        public bool HasGenericParameter
        {
            get
            {
                return true;
            }
        }

        public Type TypeForCLR
        {
            get { return typeof(ILGenericParameterType); }
        }

        public string FullName
        {
            get { return name; }
        }

        public Runtime.Enviorment.AppDomain AppDomain
        {
            get { return null; }
        }

        public Method.IMethod GetMethod(string name, int paramCount)
        {
            return null;
        }

        public Method.IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null)
        {
            return null;
        }

        public List<Method.IMethod> GetMethods()
        {
            return null;
        }

        public Method.IMethod GetConstructor(List<IType> param)
        {
            return null;
        }

        public bool CanAssignTo(IType type)
        {
            return false;
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            return null;
        }

        public IType ResolveGenericType(IType contextType)
        {
            throw new NotImplementedException();
        }


        public int GetFieldIndex(object token)
        {
            return -1;
        }


        public IType FindGenericArgument(string key)
        {
            return null;
        }


        public IType ByRefType
        {
            get { throw new NotImplementedException(); }
        }

        public IType MakeByRefType()
        {
            return this;
        }


        public IType ArrayType
        {
            get { return arrayType; }
        }

        public IType MakeArrayType()
        {
            if (arrayType == null)
                arrayType = new ILGenericParameterType(name + "[]");
            return arrayType;
        }


        public bool IsValueType
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool IsDelegate
        {
            get
            {
                return false;
            }
        }

        public Type ReflectionType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IType BaseType
        {
            get
            {
                return null;
            }
        }

        public Method.IMethod GetVirtualMethod(Method.IMethod method)
        {
            return method;
        }

        public bool IsArray
        {
            get { return false; }
        }

        public IType[] Implements
        {
            get
            {
                return null;
            }
        }
    }
}
