using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.CLR.TypeSystem
{
    class ILGenericParameterType : IType
    {
        string name;
        bool isArray, isByRef;
        ILGenericParameterType arrayType, byrefType, elementType;
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

        public bool IsGenericParameter
        {
            get
            {
                return !isByRef && !isArray;
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

        public Method.IMethod GetMethod(string name, int paramCount, bool declaredOnly = false)
        {
            return null;
        }

        public Method.IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null, bool declaredOnly = false)
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
            get { return byrefType; }
        }

        public IType MakeByRefType()
        {
            if (byrefType == null)
            {
                byrefType = new ILGenericParameterType(name + "&");
                byrefType.isByRef = true;
                byrefType.elementType = this;
            }
            return byrefType;
        }


        public IType ArrayType
        {
            get { return arrayType; }
        }

        public IType MakeArrayType(int rank)
        {
            if (arrayType == null)
            {
                arrayType = new ILGenericParameterType(name + "[]");
                arrayType.isArray = true;
                arrayType.elementType = this;
            }
            return arrayType;
        }


        public bool IsValueType
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }
        public bool IsEnum
        {
            get { return false; }
        }
        public bool IsInterface
        {
            get { return false; }
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

        public void GetValueTypeSize(out int fieldCout, out int managedCount)
        {
            throw new NotImplementedException();
        }

        public bool IsArray
        {
            get { return isArray; }
        }

        public bool IsByRef
        {
            get
            {
                return isByRef;
            }
        }

        public IType ElementType
        {
            get
            {
                return elementType;
            }
        }

        public int ArrayRank
        {
            get { return 1; }
        }

        public IType[] Implements
        {
            get
            {
                return null;
            }
        }

        public int TotalFieldCount
        {
            get
            {
                return 0;
            }
        }

        public StackObject DefaultObject { get { return default(StackObject); } }

        public int TypeIndex
        {
            get
            {
                return -1;
            }
        }
    }
}
