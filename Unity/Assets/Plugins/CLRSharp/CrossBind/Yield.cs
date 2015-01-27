using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    public class CrossBind_IEnumerable : ICrossBind
    {
        public Type Type
        {
            get { return typeof(IEnumerable); }
        }

        public object CreateBind(CLRSharp_Instance inst)
        {
            return new Base_IEnumerable(inst);
        }

        class Base_IEnumerable : IEnumerable
        {
            CLRSharp_Instance inst;
            public Base_IEnumerable(CLRSharp_Instance inst)
            {
                this.inst = inst;

            }

            public IEnumerator GetEnumerator()
            {
                var context = ThreadContext.activeContext;
                var _type = context.environment.GetType(typeof(IEnumerable));
                var _method = this.inst.type.GetMethod(_type.FullName+"."+"GetEnumerator", MethodParamList.constEmpty());
                var obj = _method.Invoke(context, inst, null) as CLRSharp_Instance;

                return context.environment.GetCrossBind(typeof(IEnumerator)).CreateBind(obj) as IEnumerator;
            }
        }
    }
    public class CrossBind_IEnumerator : ICrossBind
    {
        public Type Type
        {
            get { return typeof(IEnumerator); }
        }

        public object CreateBind(CLRSharp_Instance inst)
        {
            return new Base_IEnumerator(inst);
        }

        class Base_IEnumerator : IEnumerator
        {
            CLRSharp_Instance inst;
            public Base_IEnumerator(CLRSharp_Instance inst)
            { 
                var context = ThreadContext.activeContext;
                this.inst = inst;
                var ms = this.inst.type.GetMethodNames();
                foreach(string name in ms)
                {
                    if(name.Contains("MoveNext"))
                        _MoveNext = this.inst.type.GetMethod(name, MethodParamList.constEmpty());
                    if (name.Contains(".get_Current"))
                        _get_Current = this.inst.type.GetMethod(name, MethodParamList.constEmpty());
                    if (name.Contains(".Reset"))
                        _Reset = this.inst.type.GetMethod(name, MethodParamList.constEmpty());
                }
            }
            IMethod _MoveNext;
            IMethod _get_Current;
            IMethod _Reset;


            public object Current
            {
                get
                {
                    var context = ThreadContext.activeContext;
                    var obj = _get_Current.Invoke(context, inst, null);

                    return obj;
                }
            }

            public bool MoveNext()
            {
                var context = ThreadContext.activeContext;
                var obj = _MoveNext.Invoke(context, inst, null) as VBox;

                return obj.ToBool();
            }

            public void Reset()
            {
                var context = ThreadContext.activeContext;

                var obj = _Reset.Invoke(context, inst, null);

            }
        }
    }

}


