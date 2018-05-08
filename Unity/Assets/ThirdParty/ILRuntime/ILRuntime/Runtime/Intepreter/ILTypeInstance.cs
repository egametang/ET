using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Utils;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Intepreter
{
    public class ILTypeStaticInstance : ILTypeInstance
    {
        public unsafe ILTypeStaticInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.StaticFieldTypes.Length];
            managedObjs = new List<object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                var ft = type.StaticFieldTypes[i];
                var t = ft.TypeForCLR;
                managedObjs.Add(null);
                StackObject.Initialized(ref fields[i], i, t, ft, managedObjs);
            }
            int idx = 0;
            foreach (var i in type.TypeDefinition.Fields)
            {
                if (i.IsStatic)
                {
                    if (i.InitialValue != null && i.InitialValue.Length > 0)
                    {
                        fields[idx].ObjectType = ObjectTypes.Object;
                        managedObjs[idx] = i.InitialValue;
                    }
                    idx++;
                }
            }
        }
    }

    unsafe class ILEnumTypeInstance : ILTypeInstance
    {
        public ILEnumTypeInstance(ILType type)
        {
            if (!type.IsEnum)
                throw new NotSupportedException();
            this.type = type;
            fields = new StackObject[1];
        }

        public override string ToString()
        {
            var fields = type.TypeDefinition.Fields;
            long longVal = 0;
            int intVal = 0;
            bool isLong = this.fields[0].ObjectType == ObjectTypes.Long;
            if (isLong)
            {
                fixed (StackObject* f = this.fields)
                    longVal = *(long*)&f->Value;
            }
            else
                intVal = this.fields[0].Value;
            for (int i = 0; i < fields.Count; i++)
            {
                var f = fields[i];
                if (f.IsStatic)
                {
                    if (isLong)
                    {
                        long val = f.Constant is long ? (long)f.Constant : (long)(ulong)f.Constant;
                        if (val == longVal)
                            return f.Name;
                    }
                    else
                    {
                        if (f.Constant is int)
                        {
                            if ((int)f.Constant == intVal)
                                return f.Name;
                        }
                        else if (f.Constant is short)
                        {
                            if ((short)f.Constant == intVal)
                                return f.Name;
                        }
                        else if (f.Constant is byte)
                        {
                            if ((byte)f.Constant == intVal)
                                return f.Name;
                        }
                        else
                            throw new NotImplementedException();
                    }
                }
            }
            return isLong ? longVal.ToString() : intVal.ToString();
        }
    }

    public class ILTypeInstance
    {
        protected ILType type;
        protected StackObject[] fields;
        protected IList<object> managedObjs;
        object clrInstance;
        Dictionary<ILMethod, IDelegateAdapter> delegates;

        public ILType Type
        {
            get
            {
                return type;
            }
        }

        public StackObject[] Fields
        {
            get { return fields; }
        }

        public virtual bool IsValueType
        {
            get
            {
                return type.IsValueType && !Boxed;
            }
        }

        /// <summary>
        /// 是否已装箱
        /// </summary>
        public bool Boxed { get; set; }

        public IList<object> ManagedObjects { get { return managedObjs; } }

        public object CLRInstance { get { return clrInstance; } set { clrInstance = value; } }

        protected ILTypeInstance()
        {

        }
        public ILTypeInstance(ILType type, bool initializeCLRInstance = true)
        {
            this.type = type;
            fields = new StackObject[type.TotalFieldCount];
            managedObjs = new List<object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                managedObjs.Add(null);
            }
            InitializeFields(type);
            if (initializeCLRInstance)
            {
                if (type.FirstCLRBaseType is Enviorment.CrossBindingAdaptor)
                {
                    clrInstance = ((Enviorment.CrossBindingAdaptor)type.FirstCLRBaseType).CreateCLRInstance(type.AppDomain, this);
                }
                else
                {
                    clrInstance = this;
                }
                if(type.FirstCLRInterface is Enviorment.CrossBindingAdaptor)
                {
                    if (clrInstance != this)//Only one CLRInstance is allowed atm, so implementing multiple interfaces is not supported
                    {
                        throw new NotSupportedException("Inheriting and implementing interface at the same time is not supported yet");
                    }
                    clrInstance = ((Enviorment.CrossBindingAdaptor)type.FirstCLRInterface).CreateCLRInstance(type.AppDomain, this);
                }
            }
            else
                clrInstance = this;
        }

        public unsafe object this[int index]
        {
            get
            {
                if (index < fields.Length && index >= 0)
                {
                    fixed (StackObject* ptr = fields)
                    {
                        StackObject* esp = &ptr[index];
                        return StackObject.ToObject(esp, null, managedObjs);
                    }
                }
                else
                {
                    if (Type.FirstCLRBaseType != null && Type.FirstCLRBaseType is Enviorment.CrossBindingAdaptor)
                    {
                        CLRType clrType = type.AppDomain.GetType(((Enviorment.CrossBindingAdaptor)Type.FirstCLRBaseType).BaseCLRType) as CLRType;
                        return clrType.GetFieldValue(index, clrInstance);
                    }
                    else
                        throw new TypeLoadException();
                }
            }
            set
            {
                value = ILIntepreter.CheckAndCloneValueType(value, type.AppDomain);
                if (index < fields.Length && index >= 0)
                {
                    fixed (StackObject* ptr = fields)
                    {
                        StackObject* esp = &ptr[index];
                        if (value != null)
                        {
                            if (value.GetType().IsPrimitive)
                            {
                                ILIntepreter.UnboxObject(esp, value, managedObjs, type.AppDomain);
                            }
                            else
                            {
                                esp->ObjectType = ObjectTypes.Object;
                                esp->Value = index;
                                managedObjs[index] = value;
                            }
                        }
                        else
                            *esp = StackObject.Null;
                    }
                }
                else
                {
                    if (Type.FirstCLRBaseType != null && Type.FirstCLRBaseType is Enviorment.CrossBindingAdaptor)
                    {
                        CLRType clrType = type.AppDomain.GetType(((Enviorment.CrossBindingAdaptor)Type.FirstCLRBaseType).BaseCLRType) as CLRType;
                        clrType.SetFieldValue(index, ref clrInstance, value);
                    }
                    else
                        throw new TypeLoadException();
                }
            }
        }

        void InitializeFields(ILType type)
        {
            for (int i = 0; i < type.FieldTypes.Length; i++)
            {
                var ft = type.FieldTypes[i];
                StackObject.Initialized(ref fields[type.FieldStartIndex + i], type.FieldStartIndex + i, ft.TypeForCLR, ft, managedObjs);
            }
            if (type.BaseType != null && type.BaseType is ILType)
                InitializeFields((ILType)type.BaseType);
        }

        internal unsafe void PushFieldAddress(int fieldIdx, StackObject* esp, IList<object> managedStack)
        {
            esp->ObjectType = ObjectTypes.FieldReference;
            esp->Value = managedStack.Count;
            managedStack.Add(this);
            esp->ValueLow = fieldIdx;
        }

        internal unsafe void PushToStack(int fieldIdx, StackObject* esp, Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            if (fieldIdx < fields.Length && fieldIdx >= 0)
                PushToStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
            else
            {
                if (Type.FirstCLRBaseType != null && Type.FirstCLRBaseType is Enviorment.CrossBindingAdaptor)
                {
                    CLRType clrType = appdomain.GetType(((Enviorment.CrossBindingAdaptor)Type.FirstCLRBaseType).BaseCLRType) as CLRType;
                    ILIntepreter.PushObject(esp, managedStack, clrType.GetFieldValue(fieldIdx, clrInstance));
                }
                else
                    throw new TypeLoadException();
            }
        }

        unsafe void PushToStackSub(ref StackObject field, int fieldIdx, StackObject* esp, IList<object> managedStack)
        {
            *esp = field;
            if (field.ObjectType >= ObjectTypes.Object)
            {
                esp->Value = managedStack.Count;
                managedStack.Add(managedObjs[fieldIdx]);
            }
        }

        internal unsafe void CopyValueTypeToStack(StackObject* ptr, IList<object> mStack)
        {
            ptr->ObjectType = ObjectTypes.ValueTypeDescriptor;
            ptr->Value = type.GetHashCode();
            ptr->ValueLow = type.TotalFieldCount;
            for(int i = 0; i < fields.Length; i++)
            {
                var val = ILIntepreter.Minus(ptr, i + 1);
                switch (val->ObjectType)
                {
                    case ObjectTypes.Object:
                    case ObjectTypes.FieldReference:
                    case ObjectTypes.ArrayReference:
                        mStack[val->Value] = ILIntepreter.CheckAndCloneValueType(managedObjs[i], type.AppDomain);
                        val->ValueLow = fields[i].ValueLow;
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        {
                            var obj = managedObjs[i];
                            var dst = *(StackObject**)&val->Value;
                            var vt = type.AppDomain.GetType(dst->Value);
                            if (vt is ILType)
                            {
                                ((ILTypeInstance)obj).CopyValueTypeToStack(dst, mStack);
                            }
                            else
                            {
                                ((CLRType)vt).ValueTypeBinder.CopyValueTypeToStack(obj, dst, mStack);
                            }
                        }
                        break;
                    default:
                        *val = fields[i];
                        break;
                }                
            }
        }

        internal void Clear()
        {   
            InitializeFields(type);
        }

        internal unsafe void AssignFromStack(int fieldIdx, StackObject* esp, Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            if (fieldIdx < fields.Length && fieldIdx >= 0)
                AssignFromStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
            else
            {
                if (Type.FirstCLRBaseType != null && Type.FirstCLRBaseType is Enviorment.CrossBindingAdaptor)
                {
                    CLRType clrType = appdomain.GetType(((Enviorment.CrossBindingAdaptor)Type.FirstCLRBaseType).BaseCLRType) as CLRType;
                    var field = clrType.GetField(fieldIdx);
                    clrType.SetFieldValue(fieldIdx, ref clrInstance, field.FieldType.CheckCLRTypes(ILIntepreter.CheckAndCloneValueType(StackObject.ToObject(esp, appdomain, managedStack), appdomain)));
                }
                else
                    throw new TypeLoadException();
            }
        }

        internal unsafe void AssignFromStack(StackObject* esp, Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            StackObject* val = *(StackObject**)&esp->Value;
            int cnt = val->ValueLow;
            for (int i = 0; i < cnt; i++)
            {
                var addr = ILIntepreter.Minus(val, i + 1);
                AssignFromStack(i, addr, type.AppDomain, managedStack);
            }
        }

        unsafe void AssignFromStackSub(ref StackObject field, int fieldIdx, StackObject* esp, IList<object> managedStack)
        {
            esp = ILIntepreter.GetObjectAndResolveReference(esp);
            field = *esp;
            switch (field.ObjectType)
            {
                case ObjectTypes.Object:
                case ObjectTypes.ArrayReference:
                case ObjectTypes.FieldReference:
                    field.Value = fieldIdx;
                    managedObjs[fieldIdx] = ILIntepreter.CheckAndCloneValueType(managedStack[esp->Value], Type.AppDomain);
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var domain = type.AppDomain;
                        field.ObjectType = ObjectTypes.Object;
                        field.Value = fieldIdx;
                        var dst = *(StackObject**)&esp->Value;
                        var vt = domain.GetType(dst->Value);
                        if(vt is ILType)
                        {
                            var ins = managedObjs[fieldIdx];
                            if (ins == null)
                                throw new NullReferenceException();
                            ILTypeInstance child = (ILTypeInstance)ins;
                            child.AssignFromStack(esp, domain, managedStack);
                        }
                        else
                        {
                            managedObjs[fieldIdx] = ((CLRType)vt).ValueTypeBinder.ToObject(dst, managedStack);
                        }
                        
                    }
                    break;
                default:
                    if (managedObjs != null)
                        managedObjs[fieldIdx] = null;
                    break;
            }
        }

       
        public override string ToString()
        {
            var m = type.ToStringMethod;
            if (m != null)
            {
                if (m is ILMethod)
                {
                    var res = type.AppDomain.Invoke(m, this, null);
                    return res.ToString();
                }
                else
                    return clrInstance.ToString();
            }
            else
                return type.FullName;
        }

        public override bool Equals(object obj)
        {
            var m = type.EqualsMethod;
            if (m != null)
            {
                using (var ctx = type.AppDomain.BeginInvoke(m))
                {
                    ctx.PushObject(this);
                    ctx.PushObject(obj);
                    ctx.Invoke();
                    return ctx.ReadBool();
                }
            }
            else
            {
                if (this is ILEnumTypeInstance)
                {
                    if (obj is ILEnumTypeInstance)
                    {
                        ILEnumTypeInstance enum1 = (ILEnumTypeInstance)this;
                        ILEnumTypeInstance enum2 = (ILEnumTypeInstance)obj;
                        if (enum1.type == enum2.type)
                        {
                            var res = enum1.fields[0] == enum2.fields[0];
                            return res;
                        }
                        else
                            return false;
                    }
                    else
                        return base.Equals(obj);
                }
                else
                    return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            var m = type.GetHashCodeMethod;
            if (m != null)
            {
                using (var ctx = type.AppDomain.BeginInvoke(m))
                {
                    ctx.PushObject(this);
                    ctx.Invoke();
                    return ctx.ReadInteger();
                }
            }
            else
            {
                if (this is ILEnumTypeInstance)
                {
                    return ((ILEnumTypeInstance)this).fields[0].Value.GetHashCode();
                }
                else
                    return base.GetHashCode();
            }
        }

        public bool CanAssignTo(IType type)
        {
            return this.type.CanAssignTo(type);
        }

        public ILTypeInstance Clone()
        {
            ILTypeInstance ins = new ILTypeInstance(type);
            for (int i = 0; i < fields.Length; i++)
            {
                ins.fields[i] = fields[i];
                ins.managedObjs[i] = ILIntepreter.CheckAndCloneValueType(managedObjs[i],Type.AppDomain);
            }
            return ins;
        }

        internal IDelegateAdapter GetDelegateAdapter(ILMethod method)
        {
            if (delegates == null)
                delegates = new Dictionary<ILMethod, IDelegateAdapter>();

            IDelegateAdapter res;
            if (delegates.TryGetValue(method, out res))
                return res;
            return null;
        }

        internal void SetDelegateAdapter(ILMethod method, IDelegateAdapter adapter)
        {
            if (!delegates.ContainsKey(method))
                delegates[method] = adapter;
            else
                throw new NotSupportedException();
        }
    }
}
