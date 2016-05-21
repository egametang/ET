using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    public class RefFunc
    {
        public IMethod _method;
        public object _this;
        public RefFunc(IMethod _method, object _this)
        {
            this._method = _method;
            this._this = _this;
        }
    }
    /// <summary>
    /// 堆栈帧
    /// 一个堆栈帧，包含一个计算栈，一个临时变量槽，一个参数槽
    /// 模拟虚拟机上的堆栈帧
    /// </summary>
    public class StackFrame
    {
        public string Name
        {
            get;
            private set;
        }
        public bool IsStatic
        {
            get;
            private set;
        }
        public StackFrame(string name, bool isStatic)
        {
            this.Name = name;
            this.IsStatic = IsStatic;
        }
        Mono.Cecil.Cil.Instruction _posold;
        public void SetCodePos(int offset)
        {
            _codepos = this._body.addr[offset];
        }
        public Mono.Cecil.Cil.Instruction GetCode()
        {
            if (_body == null) return null;
            int nowaddr = this._body.opCodes[_codepos].addr;
            foreach (var c in this._body.bodyNative.Instructions)
            {
                if (nowaddr == c.Offset)
                    return c;
            }
            return null;
        }
        //public Mono.Cecil.Cil.Instruction _pos
        //{
        //    get
        //    {
        //        return _posold;
        //    }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            _codepos = -1;
        //        }
        //        else
        //        {
        //            _codepos = _body.addr[value.Offset];
        //        }
        //        _posold = value;
        //    }
        //}
        public int _codepos = 0;
        public class MyCalcStack : Stack<object>
        {
            //Queue<VBox> unused = new Queue<VBox>();
            public void Push(VBox box)
            {
                //if (box != null)
                //{
                //    box.refcount++;
                //    while (unused.Count > 0)
                //    {
                //        VBox b = unused.Dequeue();
                //        if (b.refcount == 0)
                //            ValueOnStack.UnUse(b);
                //    }
                //}
                base.Push(box);
            }
            public new object Pop()
            {
                var ob = base.Pop();
                //VBox box = ob as VBox;
                //if (box != null)
                //{
                //    box.refcount--;
                //    if (box.refcount == 0)
                //        unused.Enqueue(box);
                //}
                return ob;

            }
            public void ClearVBox()
            {
                //while (unused.Count > 0)
                //{
                //    VBox b = unused.Dequeue();
                //    if (b.refcount == 0)
                //    {
                //        ValueOnStack.UnUse(b);
                //    }
                //    else
                //    {
                //        Console.WriteLine("not zero.");
                //    }
                //}
                this.Clear();
            }
        }
        public MyCalcStack stackCalc = new MyCalcStack();
        public class MySlotVar : List<object>
        {
            public new void Add(object obj)
            {
                base.Add(obj);
            }
            public void Add(VBox box)
            {
                //if (box != null)
                //{
                //    box.refcount++;
                //}
                base.Add(box);
            }
            public void ClearVBox()
            {
                foreach (object b in this)
                {
                    VBox box = b as VBox;
                    if (box != null)
                    {
                        ValueOnStack.UnUse(box);
                    }
                }
                //        box.refcount--;
                //        if (box.refcount == 0)
                //        {
                //            ValueOnStack.UnUse(box);
                //        }
                //        else
                //        {
                //            Console.WriteLine("not zero.");
                //        }
                //    }
                //}
                this.Clear();
            }
        }
        public MySlotVar slotVar = new MySlotVar();
        public object[] _params = null;
        public void SetParams(object[] _p)
        {
            if (_p == null)
            {
                _params = null;
                return;
            }
            _params = new object[_p.Length];
            for (int i = 0; i < _p.Length; i++)
            {
                if (_p[i] != null)
                {
                    var vbox = ValueOnStack.MakeVBox(_p[i].GetType());
                    if (vbox != null)
                    {
                        vbox.SetDirect(_p[i]);
                        _params[i] = vbox;
                        continue;
                    }
                }

                _params[i] = _p[i];
            }
        }
        CodeBody _body = null;
        public CodeBody codebody
        {
            get
            {
                return _body;
            }
        }
        public void Init(CodeBody body)
        {
            _body = body;
            if (body.typelistForLoc != null)
            {
                for (int i = 0; i < body.typelistForLoc.Count; i++)
                {
                    ICLRType t = _body.typelistForLoc[i];

                    slotVar.Add(ValueOnStack.MakeVBox(t));

                }
            }
        }
        public object Return()
        {

            this.slotVar.ClearVBox();
            if (this.stackCalc.Count == 0) return null;
            object ret = stackCalc.Pop();
            this.stackCalc.ClearVBox();

            return ret;
        }
        void FillArray(object array, byte[] bytes)
        {
            if (bytes == null)
                return;
            if (array is byte[])
            {
                byte[] arr = array as byte[];
                for (int i = 0; i < bytes.Length; i++)
                {
                    arr[i] = bytes[i];
                }
            }
            else if (array is sbyte[])
            {
                sbyte[] arr = array as sbyte[];
                for (int i = 0; i < bytes.Length; i++)
                {
                    arr[i] = (sbyte)bytes[i];
                }
            }
            else if (array is Int16[])
            {
                int step = 2;
                Int16[] arr = array as Int16[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToInt16(bytes, i * step);
                }
            }
            else if (array is UInt16[])
            {
                int step = 2;
                UInt16[] arr = array as UInt16[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToUInt16(bytes, i * step);
                }
            }
            else if (array is char[])
            {
                int step = 2;
                char[] arr = array as char[];

                for (int i = 0; i < Math.Min(bytes.Length / step, arr.Length); i++)
                {
                    arr[i] = (char)BitConverter.ToUInt16(bytes, i * step);
                }
            }
            else if (array is int[])
            {
                int step = 4;
                int[] arr = array as int[];

                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToInt32(bytes, i * step);
                }
            }
            else if (array is uint[])
            {
                int step = 4;
                uint[] arr = array as uint[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToUInt32(bytes, i * step);
                }
            }
            else if (array is Int64[])
            {
                int step = 8;
                Int64[] arr = array as Int64[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToInt64(bytes, i * step);
                }
            }
            else if (array is UInt64[])
            {
                int step = 8;
                UInt64[] arr = array as UInt64[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToUInt64(bytes, i * step);
                }
            }
            else if (array is float[])
            {
                int step = 4;
                float[] arr = array as float[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToSingle(bytes, i * step);
                }
            }
            else if (array is double[])
            {
                int step = 8;
                double[] arr = array as double[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToDouble(bytes, i * step);
                }
            }
            else if (array is bool[])
            {
                int step = 1;
                bool[] arr = array as bool[];
                for (int i = 0; i < bytes.Length / step; i++)
                {
                    arr[i] = BitConverter.ToBoolean(bytes, i * step);
                }
            }
            else
            {
                throw new NotImplementedException("array=" + array.GetType());
            }
        }
        //流程控制
        public void Call(ThreadContext context, IMethod _clrmethod, bool bVisual)
        {

            if (_clrmethod == null)//不想被执行的函数
            {
                _codepos++;
                return;
            }

            object[] _pp = null;
            object _this = null;
            bool bCLR = _clrmethod is IMethod_Sharp;
            if (_clrmethod.ParamList != null)
            {
                _pp = new object[_clrmethod.ParamList.Count];
                for (int i = 0; i < _pp.Length; i++)
                {
                    int iCallPPos = _pp.Length - 1 - i;
                    ICLRType pType = _clrmethod.ParamList[iCallPPos];
                    var pp = stackCalc.Pop();
                    if (pp is CLRSharp_Instance && pType.TypeForSystem != typeof(CLRSharp_Instance))
                    {
                        var inst = pp as CLRSharp_Instance;

                        var btype = inst.type.ContainBase(pType.TypeForSystem);
                        if (btype)
                        {
                            var CrossBind = context.environment.GetCrossBind(pType.TypeForSystem);
                            if (CrossBind != null)
                            {
                                pp = CrossBind.CreateBind(inst);
                            }
                            else
                            {
                                pp = inst.system_base;
                                //如果没有绑定器，尝试直接使用System_base;
                            }
                            //context.environment.logger.Log("这里有一个需要映射的类型");
                        }

                    }
                    if (pp is VBox && !bCLR)
                    {
                        ValueOnStack.UnUse(pp as VBox);
                        pp = (pp as VBox).BoxDefine();
                    }
                    if (pp is ICLRType_System)
                    {
                        pp = (pp as ICLRType_System).TypeForSystem;
                    }
                    if ((pp is int) && (pType.TypeForSystem != typeof(int) && pType.TypeForSystem != typeof(object)))
                    {
                        var _vbox = ValueOnStack.MakeVBox(pType);
                        if (_vbox != null)
                        {
                            _vbox.SetDirect(pp);
                            if (bCLR)
                                pp = _vbox;
                            else
                                pp = _vbox.BoxDefine();
                        }
                    }
                    _pp[iCallPPos] = pp;
                }
            }


            //if (method.HasThis)
            if (!_clrmethod.isStatic)
            {
                _this = stackCalc.Pop();
            }
            if (_clrmethod.DeclaringType.FullName.Contains("System.Runtime.CompilerServices.RuntimeHelpers") && _clrmethod.Name.Contains("InitializeArray"))
            {

                byte[] bb = _pp[1] as byte[];
                if (bb == null && _pp[1] is CLRSharp.Field_Common_CLRSharp)
                {
                    bb = (_pp[1] as CLRSharp.Field_Common_CLRSharp).field.InitialValue;
                }
                FillArray(_pp[0], bb);
                _codepos++;
                return;
            }
            if (_clrmethod.DeclaringType.FullName.Contains("System.Type") && _clrmethod.Name.Contains("GetTypeFromHandle"))
            {
                stackCalc.Push(_pp[0]);
                _codepos++;
                return;
            }
            if (_clrmethod.DeclaringType.FullName.Contains("System.Object") && _clrmethod.Name.Contains(".ctor"))
            {//跳过这个没意义的构造
                _codepos++;
                return;
            }
            if (_this is RefObj && _clrmethod.Name != ".ctor")
            {
                _this = (_this as RefObj).Get();

            }
            if (_this is VBox)
            {
                //ValueOnStack.UnUse(_this as VBox);//放在这里会导致计算错误，原因待查

                _this = (_this as VBox).BoxDefine();
            }
            bool bCross = (_this is CLRSharp_Instance && _clrmethod is IMethod_System);
            object returnvar = _clrmethod.Invoke(context, _this, _pp, bVisual);

            if(_this is VBox)
            {
                ValueOnStack.UnUse(_this as VBox);
            }
            if (bCross)
            {
                //这里究竟如何处理还需要再考虑
                //returnvar = _clrmethod.Invoke(context, (_this as CLRSharp_Instance).system_base, _pp, bVisual);
                if (_clrmethod.Name.Contains(".ctor"))
                {
                    (_this as CLRSharp_Instance).system_base = returnvar;
                    returnvar = (_this);
                }
            }
            else
            {
                //returnvar = _clrmethod.Invoke(context, _this, _pp, bVisual);
            }


            // bool breturn = false;
            if (_clrmethod.ReturnType != null && _clrmethod.ReturnType.FullName != "System.Void")
            {
                if ((returnvar is VBox) == false)
                {
                    var type = ValueOnStack.MakeVBox(_clrmethod.ReturnType);
                    if (type != null)
                    {
                        type.SetDirect(returnvar);
                        returnvar = type;
                    }
                }
                stackCalc.Push(returnvar);
            }

            else if (_this is RefObj && _clrmethod.Name == ".ctor")
            {
                //如果这里有发生程序类型，脚本类型的cross，就需要特别处理
                (_this as RefObj).Set(returnvar);
            }
            _codepos++;
            return;

        }
        //栈操作
        public void Nop()
        {
            _codepos++;
        }
        public void Dup()
        {
            var v = stackCalc.Peek();
            if (v is VBox)
            {
                v = (v as VBox).Clone();
            }
            stackCalc.Push(v);
            _codepos++;
        }
        public void Pop()
        {
            var v = stackCalc.Pop() as VBox;
            ValueOnStack.UnUse(v);
            _codepos++;
        }
        //流程控制
        public void Ret()
        {
            _codepos++;
        }
        public void Box(ICLRType type)
        {
            object obj = stackCalc.Pop();
            VBox box = obj as VBox;
            if (type.TypeForSystem.IsEnum)
            {
                int ev = 0;
                if (box != null) ev = box.v32;
                else ev = (int)obj;
                obj = Enum.ToObject(type.TypeForSystem, ev);
            }
            else
            {
                if (box != null)
                {
                    var tcode = ValueOnStack.GetTypeCode(type.TypeForSystem);
                    if (tcode == box.type)
                    {


                        obj = box.BoxDefine();
                        ValueOnStack.UnUse(box);

                    }
                    else
                    {
                        var nbox = ValueOnStack.MakeVBox(tcode);
                        nbox.Set(box);
                        obj = nbox.BoxDefine();
                        ValueOnStack.UnUse(nbox);
                        ValueOnStack.UnUse(box);
                    }
                }
            }
            stackCalc.Push(obj);
            _codepos++;
        }
        public void Unbox()
        {
            object obj = stackCalc.Pop();
            var box = ValueOnStack.MakeVBox(obj.GetType());
            if (box != null)
            {
                box.SetDirect(obj);
                stackCalc.Push(box);
            }
            else
            {
                stackCalc.Push(obj);
            }
            _codepos++;
        }
        public void Unbox_Any()
        {
            object obj = stackCalc.Pop();
            var box = ValueOnStack.MakeVBox(obj.GetType());
            if (box != null)
            {
                box.SetDirect(obj);
                stackCalc.Push(box);
            }
            else
            {
                stackCalc.Push(obj);
            }

            _codepos++;
        }
        public void Br(int addr_index)
        {
            _codepos = addr_index;// _body.addr[pos.Offset];
        }
        public void Leave(int addr_index)
        {
            stackCalc.Clear();
            _codepos = addr_index;// _body.addr[addr];
        }
        //public void Leave_AddrIndex(int addr_index)
        //{
        //    stackCalc.Clear();
        //    _codepos = addr_index;
        //}
        public void Brtrue(int addr_index)
        {
            object obj = stackCalc.Pop();
            bool b = false;
            if (obj != null)
            {
                if (obj is VBox)
                {
                    VBox box = obj as VBox;
                    b = box.ToBool();
                    ValueOnStack.UnUse(box);
                }
                else if (obj.GetType().IsClass)
                {
                    b = true;
                }
                else if (obj is bool)
                {
                    b = (bool)obj;
                }
                else
                {
                    b = Convert.ToDecimal(obj) > 0;
                }
            }
            //decimal b = Convert.ToDecimal(stackCalc.Pop());
            //bool b = (bool)stackCalc.Pop();
            if (b)
            {
                _codepos = addr_index;// _body.addr[pos.Offset];
            }
            else
            {
                _codepos++;
            }
        }
        public void Brfalse(int addr_index)
        {
            object obj = stackCalc.Pop();
            bool b = false;
            if (obj != null)
            {
                if (obj is VBox)
                {
                    VBox box = obj as VBox;
                    b = box.ToBool();
                    ValueOnStack.UnUse(box);
                }
                else if (obj.GetType().IsClass)
                {
                    b = true;
                }
                else if (obj is bool)
                {
                    b = (bool)obj;
                }
                else
                {
                    b = Convert.ToDecimal(obj) > 0;
                }
            }
            if (!b)
            {
                _codepos = addr_index;// _body.addr[pos.Offset];
            }
            else
            {
                _codepos++;
            }
        }
        //条件跳转
        public void Beq(int addr_index)
        {
            object o2 = stackCalc.Pop();
            object o1 = stackCalc.Pop();
            if (o1 is VBox && o2 is VBox)
            {
                VBox n2 = o2 as VBox;
                VBox n1 = o1 as VBox;

                if (n1.logic_eq(n2))
                {
                    _codepos = addr_index;// _body.addr[pos.Offset];
                }
                else
                {
                    _codepos++;
                }
                ValueOnStack.UnUse(n1);
                ValueOnStack.UnUse(n2);
            }
            else if (o1 is int)
            {

                int lv = (int)o1;
                int rv = 0;
                if (o2 is int)
                    rv = (int)o2;
                else if (o2 is VBox)
                {
                    rv = (o2 as VBox).v32;
                    ValueOnStack.UnUse(o2 as VBox);

                }
                else
                    throw new Exception("what a fuck");

                if (lv == rv)
                {
                    _codepos = addr_index;// _body.addr[pos.Offset];
                }
                else
                {
                    _codepos++;
                }
            }
            else
            {
                throw new Exception("what a fuck");
            }

        }
        public void Bne(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_ne(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];
            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Bne_Un(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_ne_Un(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Bge(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_ge(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Bge_Un(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_ge_Un(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Bgt(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_gt(n2))
            {
                _codepos = addr_index;//_body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Bgt_Un(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_gt_Un(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Ble(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_le(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Ble_Un(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_le_Un(n2))
            {
                _codepos = addr_index;//_body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Blt(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_lt(n2))
            {
                _codepos = addr_index;//_body.addr[pos.Offset];

            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        public void Blt_Un(int addr_index)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;

            if (n1.logic_lt_Un(n2))
            {
                _codepos = addr_index;// _body.addr[pos.Offset];
            }
            else
            {
                _codepos++;
            }
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);

        }
        //加载常量
        public void Ldc_I4(int v)//int32
        {
            VBox box = ValueOnStack.MakeVBox(NumberType.INT32);
            box.v32 = v;
            stackCalc.Push(box);
            _codepos++;

        }

        public void Ldc_I8(Int64 v)//int64
        {
            VBox box = ValueOnStack.MakeVBox(NumberType.INT64);
            box.v64 = v;
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldc_R4(float v)
        {
            VBox box = ValueOnStack.MakeVBox(NumberType.FLOAT);
            box.vDF = v;
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldc_R8(double v)
        {
            VBox box = ValueOnStack.MakeVBox(NumberType.DOUBLE);
            box.vDF = v;
            stackCalc.Push(box);
            _codepos++;
        }
        //放进变量槽
        public void Stloc(int pos)
        {
            object v = stackCalc.Pop();
            while (slotVar.Count <= pos)
            {
                slotVar.Add(null);
            }
            if (v != null && v.GetType().IsValueType)
            {
                var mm = v.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                slotVar[pos] = mm.Invoke(v, null);
            }
            else
            {
                VBox box = slotVar[pos] as VBox;
                if (box == null)
                {
                    slotVar[pos] = v;
                }
                else
                {
                    if (v is VBox)
                    {
                        box.Set(v as VBox);
                    }
                    else
                        box.SetDirect(v);
                }
            }
            if (v is VBox)
            {
                ValueOnStack.UnUse(v as VBox);
            }
            _codepos++;
        }
        //拿出变量槽
        public void Ldloc(int pos)
        {
            var obj = slotVar[pos];
            VBox b = obj as VBox;
            if (b != null)
            {
                obj = b.Clone();
            }
            stackCalc.Push(obj);
            _codepos++;
        }
        public enum RefType
        {
            loc,//本地变量槽
            arg,//参数槽
            field,//成员变量
            Array,
        }
        public class RefObj
        {
            public StackFrame frame;
            public int pos;
            public RefType type;
            //public ICLRType _clrtype;
            public IField _field;
            public object _this;
            public Array _array;
            public RefObj(StackFrame frame, int pos, RefType type)
            {
                this.frame = frame;
                this.pos = pos;
                this.type = type;
            }
            public RefObj(IField field, object _this)
            {
                this.type = RefType.field;
                //this._clrtype = type;
                this._field = field;
                this._this = _this;
            }
            public RefObj(Array array, int index)
            {
                this.type = RefType.Array;
                this._array = array;
                this.pos = index;
            }
            public void Set(object obj)
            {
                if (type == RefType.arg)
                {
                    frame._params[pos] = obj;
                }
                else if (type == RefType.loc)
                {
                    while (frame.slotVar.Count <= pos)
                    {
                        frame.slotVar.Add(null);
                    }
                    frame.slotVar[pos] = obj;
                }
                else if (type == RefType.field)
                {
                    _field.Set(_this, obj);
                }
                else if (type == RefType.Array)
                {
                    _array.SetValue(obj, pos);
                }

            }
            public object Get()
            {
                if (type == RefType.arg)
                {
                    return frame._params[pos];
                }
                else if (type == RefType.loc)
                {
                    while (frame.slotVar.Count <= pos)
                    {
                        frame.slotVar.Add(null);
                    }
                    return frame.slotVar[pos];
                }
                else if (type == RefType.field)
                {
                    return _field.Get(_this);
                }
                else if (type == RefType.Array)
                {
                    return _array.GetValue(pos);
                }
                return null;
            }

        }

        //拿出变量槽的引用

        public void Ldloca(int pos)
        {
            stackCalc.Push(new RefObj(this, pos, RefType.loc));
            _codepos++;
        }

        public void Ldstr(string text)
        {
            stackCalc.Push(text);
            _codepos++;
        }

        //加载参数(还得处理static，il静态非静态不一样，成员参数0是this)
        public void Ldarg(int pos)
        {
            object obj = null;
            if (_params != null)
                obj = _params[pos];
            VBox b = obj as VBox;
            if (b != null)
            {
                obj = b.Clone();
            }
            stackCalc.Push(obj);
            _codepos++;
        }
        public void Ldarga(int pos)
        {
            stackCalc.Push(new RefObj(this, pos, RefType.arg));
            _codepos++;
        }
        //逻辑计算

        public void Ceq()
        {
            var obj2 = stackCalc.Pop();
            var obj1 = stackCalc.Pop();
            VBox n2 = obj2 as VBox;
            VBox n1 = obj1 as VBox;
            bool beq = false;
            if (obj1 == null || obj2 == null)
            {
                beq = obj1 == obj2;
            }
            else if (n1 == null || n2 == null)

            {
                if (n1 != null)
                {
                    n2 = ValueOnStack.MakeVBox(obj2.GetType());
                    n2.SetDirect(obj2);
                    beq = n1.logic_eq(n2);
                }
                else if (n2 != null)
                {
                    n1 = ValueOnStack.MakeVBox(obj1.GetType());
                    n1.SetDirect(obj1);
                    beq = n1.logic_eq(n2);
                }
                else
                {
                    if (obj1 != null)
                        beq = obj1.Equals(obj2);
                    else
                        beq = (obj1 == obj2);
                }
            }
            else
            {
                beq = n1.logic_eq(n2);
            }


            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);
            stackCalc.Push(ValueOnStack.MakeVBoxBool(beq));
            _codepos++;
        }
        public void Cgt()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;


            stackCalc.Push(ValueOnStack.MakeVBoxBool(n1.logic_gt(n2)));
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);
            _codepos++;
        }
        public void Cgt_Un()
        {
            object _n2 = stackCalc.Pop();
            object _n1 = stackCalc.Pop();
            if (_n2 == null)
            {
                stackCalc.Push(ValueOnStack.MakeVBoxBool(_n1 != null));
                _codepos++;
            }
            else
            {
                VBox n2 = GetVBox(_n2);
                VBox n1 = GetVBox(_n1);

                stackCalc.Push(ValueOnStack.MakeVBoxBool(n1.logic_gt_Un(n2)));
                ValueOnStack.UnUse(n1);
                ValueOnStack.UnUse(n2);
                _codepos++;
            }
        }
        public void Clt()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            stackCalc.Push(ValueOnStack.MakeVBoxBool(n1.logic_lt(n2)));
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);
            _codepos++;
        }
        public void Clt_Un()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            stackCalc.Push(ValueOnStack.MakeVBoxBool(n1.logic_lt_Un(n2)));
            ValueOnStack.UnUse(n1);
            ValueOnStack.UnUse(n2);
            _codepos++;
        }
        public void Ckfinite()
        {
            object n1 = stackCalc.Pop();
            if (n1 is float)
            {
                float v = (float)n1;
                stackCalc.Push(float.IsInfinity(v) || float.IsNaN(v) ? 1 : 0);
            }
            else
            {
                double v = (double)n1;
                stackCalc.Push(double.IsInfinity(v) || double.IsNaN(v) ? 1 : 0);
            }
            _codepos++;
        }
        //算术操作
        public void Add()
        {
            VBox n2 = GetVBox(stackCalc.Pop());
            object n1 = stackCalc.Pop();
            if (n1 is VBox)
            {
                VBox n_1 = n1 as VBox;
                n_1.Add(n2);
                stackCalc.Push(n_1);
            }
            else
            {
                VBox n_1 = GetVBox(n1);
                n_1.Add(n2);
                stackCalc.Push(n_1.BoxDefine());
                ValueOnStack.UnUse(n_1);

            }
            ValueOnStack.UnUse(n2);
            _codepos++;
        }

        //Modify by hjx
        public VBox GetVBox(object obj)
        {
            VBox box = null;
            if (obj is VBox)
            {
                box = obj as VBox;
            }
            else
            {
                box = ValueOnStack.MakeVBox(obj.GetType());
                box.SetDirect(obj);
            }
            return box;
        }










        public void Sub()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            object n1 = stackCalc.Pop();
            if (n1 is VBox)
            {
                VBox n_1 = n1 as VBox;
                n_1.Sub(n2);
                stackCalc.Push(n_1);
            }
            else
            {
                VBox n_1 = ValueOnStack.MakeVBox(n1.GetType());
                n_1.SetDirect(n1);
                n_1.Sub(n2);
                stackCalc.Push(n_1.BoxDefine());
                ValueOnStack.UnUse(n_1);

            }
            ValueOnStack.UnUse(n2);

            _codepos++;
        }
        public void Mul()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Mul(n2);
            stackCalc.Push(n1);
            ValueOnStack.UnUse(n2);

            _codepos++;
        }
        public void Div()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Div(n2);
            stackCalc.Push(n1);
            ValueOnStack.UnUse(n2);

            _codepos++;
        }
        public void Div_Un()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Div(n2);//!!! _un
            stackCalc.Push(n1);
            ValueOnStack.UnUse(n2);

            _codepos++;
        }
        public void Rem()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Mod(n2);
            stackCalc.Push(n1);
            ValueOnStack.UnUse(n2);

            _codepos++; ;
        }
        public void Rem_Un()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Mod(n2);//!!!_un
            stackCalc.Push(n1);
            ValueOnStack.UnUse(n2);

            _codepos++;
        }
        public void Neg()
        {

            object n1 = stackCalc.Pop();
            if (n1 is VBox)
            {
                VBox v = n1 as VBox;
                var obj = v.Clone();
                obj.Neg();
                stackCalc.Push(obj);
                ValueOnStack.UnUse(v);


            }
            else if (n1 is int)
            {
                stackCalc.Push(-(int)n1);
            }
            else if (n1 is Int64)
            {
                stackCalc.Push(-(Int64)n1);
            }

            else
            {
                stackCalc.Push(n1);
            }

            _codepos++;
        }
        //转换
        public void Conv_I1()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.SBYTE));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((sbyte)num1);
            }
            _codepos++;
        }
        public void Conv_U1()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.BYTE));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((byte)num1);
            }
            _codepos++;
        }
        public void Conv_I2()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT16));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((Int16)num1);
            }
            _codepos++;
        }
        public void Conv_U2()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT16));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((UInt16)num1);
            }
            _codepos++;
        }
        public void Conv_I4()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT32));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((Int32)num1);
            }
            _codepos++;
        }
        public void Conv_U4()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT32));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((UInt32)num1);
            }
            _codepos++;
        }
        public void Conv_I8()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT64));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((Int64)num1);
            }
            _codepos++;
        }
        public void Conv_U8()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT64));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((UInt64)num1);
            }
            _codepos++;
        }
        public void Conv_I()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT32));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((Int32)num1);
            }
            _codepos++;
        }
        public void Conv_U()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT32));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((UInt32)num1);
            }
            _codepos++;
        }
        public void Conv_R4()
        {


            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.FLOAT));
                ValueOnStack.UnUse(b);
            }
            else
            {
                if (num1.GetType() == typeof(double))
                {
                    //Modify by hjx
                    stackCalc.Push((float)(double)num1);
                }
                else
                {
                    stackCalc.Push((float)num1);
                }
            }
            _codepos++;
        }
        public void Conv_R8()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.DOUBLE));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((double)num1);
            }
            _codepos++;
        }
        public void Conv_R_Un()
        {

            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.FLOAT));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((float)num1);
            }
            _codepos++;
        }

        ////数组
        public void NewArr(ThreadContext context, Type type)
        {
            //string typename = type.FullName + "[]";
            //var _type = context.environment.GetType(typename, type.Module);
            //MethodParamList tlist = MethodParamList.MakeList_OneParam_Int(context.environment);
            //var m = _type.GetMethod(".ctor", tlist);
            var objv = stackCalc.Pop();


            if (objv is VBox)
            {
                ValueOnStack.UnUse(objv as VBox);
                objv = (objv as VBox).BoxDefine();
            }
            var array = Array.CreateInstance(type, (int)objv);
            stackCalc.Push(array);
            _codepos++;
        }
        public void LdLen()
        {
            var obj = stackCalc.Pop();
            Array a = obj as Array;
            var vbox = ValueOnStack.MakeVBox(NumberType.INT32);
            vbox.v32 = a.Length;
            stackCalc.Push(vbox);
            _codepos++;
        }
        public void Ldelema(object obj)
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            Array array = stackCalc.Pop() as Array;


            stackCalc.Push(new RefObj(array, index));
            _codepos++;
            //_codepos++;
        }
        public void Ldelem_I1()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var _array = stackCalc.Pop();
            if (_array is sbyte[])
            {
                sbyte[] array = _array as sbyte[]; ;
                var box = ValueOnStack.MakeVBox(NumberType.SBYTE);
                box.v32 = array[index];
                stackCalc.Push(box);
            }
            else if (_array is bool[])
            {
                bool[] array = _array as bool[]; ;
                var box = ValueOnStack.MakeVBox(NumberType.BOOL);
                box.v32 = array[index] == true ? 1 : 0;
                stackCalc.Push(box);
            }
            else
            {
                throw new Exception("not support.this array i1");
            }

            _codepos++;
        }
        public void Ldelem_U1()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            object obj = stackCalc.Pop();
            if (obj is byte[])
            {
                byte[] array = obj as byte[];
                var box = ValueOnStack.MakeVBox(NumberType.BYTE);
                box.v32 = array[index];
                stackCalc.Push(box);
                _codepos++;
            }
            else if (obj is bool[])
            {
                bool[] array = obj as bool[];
                var box = ValueOnStack.MakeVBox(NumberType.BOOL);
                box.v32 = array[index] ? 1 : 0;
                stackCalc.Push(box);
                _codepos++;
            }
        }

        public void Ldelem_I2()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            Int16[] array = stackCalc.Pop() as Int16[];
            var box = ValueOnStack.MakeVBox(NumberType.INT16);
            box.v32 = array[index];
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldelem_U2()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var _array = stackCalc.Pop();
            if (_array is UInt16[])
            {
                UInt16[] array = _array as UInt16[];
                var box = ValueOnStack.MakeVBox(NumberType.UINT16);
                box.v32 = array[index];
                stackCalc.Push(box);
            }
            else
            {
                char[] array = _array as char[];
                var box = ValueOnStack.MakeVBox(NumberType.CHAR);
                box.v32 = array[index];
                stackCalc.Push(box);
            }

            _codepos++;
        }
        public void Ldelem_I4()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            int[] array = stackCalc.Pop() as int[];
            var box = ValueOnStack.MakeVBox(NumberType.INT32);
            box.v32 = array[index];
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldelem_U4()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            uint[] array = stackCalc.Pop() as uint[];
            var box = ValueOnStack.MakeVBox(NumberType.UINT32);
            box.v32 = (int)array[index];
            stackCalc.Push(box);
            _codepos++;
        }

        public void Ldelem_I8()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var obj = stackCalc.Pop();
            if (obj is Int64[])
            {
                Int64[] array = obj as Int64[];
                var box = ValueOnStack.MakeVBox(NumberType.INT64);
                box.v64 = array[index];
                stackCalc.Push(box);
            }
            else
            {
                UInt64[] array = obj as UInt64[];
                var box = ValueOnStack.MakeVBox(NumberType.INT64);
                box.v64 = (long)array[index];
                stackCalc.Push(box);
            }
            _codepos++;
        }
        public void Ldelem_I()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            int[] array = stackCalc.Pop() as int[];
            var box = ValueOnStack.MakeVBox(NumberType.INT32);
            box.v32 = array[index];
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldelem_R4()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            float[] array = stackCalc.Pop() as float[];
            var box = ValueOnStack.MakeVBox(NumberType.FLOAT);
            box.vDF = array[index];
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldelem_R8()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            double[] array = stackCalc.Pop() as double[];
            var box = ValueOnStack.MakeVBox(NumberType.DOUBLE);
            box.vDF = array[index];
            stackCalc.Push(box);
            _codepos++;
        }
        public void Ldelem_Ref()
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            Array array = stackCalc.Pop() as Array;
            stackCalc.Push(array.GetValue(index));
            _codepos++;
        }
        public void Ldelem_Any(object obj)
        {
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            Array array = stackCalc.Pop() as Array;

            stackCalc.Push(array.GetValue(index));
            _codepos++;
        }
        public void Stelem_I()
        {
            Stelem_I4();
            //var obj = stackCalc.Pop();
            //int value = 0;
            //if (obj is VBox)
            //{
            //    value = (obj as VBox).ToInt();
            //}
            //else
            //{
            //    value = (Int32)obj;
            //}
            //var indexobj = stackCalc.Pop();
            //int index = 0;
            //if ((indexobj is VBox))
            //{
            //    index = (indexobj as VBox).ToInt();
            //}
            //else
            //{
            //    index = (int)indexobj;
            //}
            //var array = stackCalc.Pop() as Int32[];
            //array[index] = value;
            //_codepos++;
        }
        public void Stelem_I1()
        {
            var obj = stackCalc.Pop();
            int value = 0;
            if (obj is VBox)
            {
                value = (obj as VBox).ToInt();
                ValueOnStack.UnUse(obj as VBox);
            }
            else
            {
                value = (sbyte)obj;
            }
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var array = stackCalc.Pop();
            if (array is sbyte[])
            {
                (array as sbyte[])[index] = (sbyte)value;
            }
            else if (array is byte[])
            {
                (array as byte[])[index] = (byte)value;
            }
            else if (array is bool[])
            {
                (array as bool[])[index] = value > 0;

            }

            _codepos++;
        }
        public void Stelem_I2()
        {
            var obj = stackCalc.Pop();
            int value = 0;
            if (obj is VBox)
            {
                value = (obj as VBox).ToInt();
                ValueOnStack.UnUse(obj as VBox);
            }
            else
            {
                value = (Int16)obj;
            }
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var array = stackCalc.Pop();
            if (array is char[])
            {
                (array as char[])[index] = (char)value;
            }
            else if (array is Int16[])
            {
                (array as Int16[])[index] = (Int16)value;
            }
            else if (array is UInt16[])
            {
                (array as UInt16[])[index] = (UInt16)value;
            }

            _codepos++;
        }
        public void Stelem_I4()
        {
            var obj = stackCalc.Pop();
            int value = 0;
            if (obj is VBox)
            {
                value = (obj as VBox).ToInt();
                ValueOnStack.UnUse(obj as VBox);
            }
            else
            {
                value = (Int32)obj;
            }
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var _array = stackCalc.Pop();
            if (_array is Int32[])
            {

                var array = _array as Int32[];
                array[index] = (Int32)value;

            }
            else if (_array is UInt32[])
            {
                var array = _array as UInt32[];
                array[index] = (UInt32)value;

            }
            _codepos++;
        }
        public void Stelem_I8()
        {
            var obj = stackCalc.Pop();
            long value = 0;
            if (obj is VBox)
            {
                value = (obj as VBox).ToInt64();
                ValueOnStack.UnUse(obj as VBox);
            }
            else
            {
                value = (Int64)obj;
            }
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else
            {
                index = (int)indexobj;
            }
            var _array = stackCalc.Pop();
            if (_array is Int64[])
            {

                var array = _array as Int64[];
                array[index] = (Int64)value;

            }
            else if (_array is UInt64[])
            {
                var array = _array as UInt64[];
                array[index] = (UInt64)value;

            }
            _codepos++;
        }
        public void Stelem_R4()
        {
            var obj = stackCalc.Pop();
            float value = 0;
            if (obj is VBox)
            {
                value = (obj as VBox).ToFloat();
                ValueOnStack.UnUse(obj as VBox);

            }
            else
            {
                value = (float)obj;
            }
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);

            }
            else
            {
                index = (int)indexobj;
            }
            var array = stackCalc.Pop() as float[];
            array[index] = value;
            _codepos++;
        }
        public void Stelem_R8()
        {
            var obj = stackCalc.Pop();
            double value = 0;
            if (obj is VBox)
            {
                value = (obj as VBox).ToDouble();
                ValueOnStack.UnUse(obj as VBox);

            }
            else
            {
                value = (double)obj;
            }
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);

            }
            else
            {
                index = (int)indexobj;
            }
            var array = stackCalc.Pop() as double[];
            array[index] = value;
            _codepos++;
        }
        public void Stelem_Ref()
        {
            var value = stackCalc.Pop();
            var indexobj = stackCalc.Pop();
            int index = 0;
            if ((indexobj is VBox))
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);

            }
            else
            {
                index = (int)indexobj;
            }
            var array = stackCalc.Pop() as Object[];

            array[index] = value;
            _codepos++;
        }

        public void Stelem_Any()
        {
            var value = stackCalc.Pop();
            var indexobj = stackCalc.Pop();
            int index = 0;
            if (indexobj is VBox)
            {
                index = (indexobj as VBox).ToInt();
                ValueOnStack.UnUse(indexobj as VBox);

            }
            else index = (int)indexobj;
            var array = stackCalc.Pop() as Array;
            array.SetValue(value, index);
            _codepos++;
        }

        //寻址类
        public void NewObj(ThreadContext context, IMethod _clrmethod)
        {
            //MethodParamList list = new MethodParamList(context.environment, method);
            object[] _pp = null;
            bool bCLR = _clrmethod is IMethod_Sharp;
            if (_clrmethod.ParamList != null)
            {
                _pp = new object[_clrmethod.ParamList.Count];
                for (int i = 0; i < _pp.Length; i++)
                {
                    int iCallPPos = _pp.Length - 1 - i;
                    ICLRType pType = _clrmethod.ParamList[iCallPPos];
                    var pp = stackCalc.Pop();
                    if (pp is VBox && !bCLR)
                    {
                        ValueOnStack.UnUse(pp as VBox);
                        pp = (pp as VBox).BoxDefine();
                    }
                    if ((pp is int) && (pType.TypeForSystem != typeof(int) && pType.TypeForSystem != typeof(object)))
                    {
                        var _vbox = ValueOnStack.MakeVBox(pType);
                        if (_vbox != null)
                        {
                            _vbox.SetDirect(pp);
                            if (bCLR)
                                pp = _vbox;
                            else
                                pp = _vbox.BoxDefine();
                        }
                    }
                    _pp[iCallPPos] = pp;
                }
            }

            //var typesys = context.environment.GetType(method.DeclaringType.FullName, method.Module);
            object returnvar = _clrmethod.Invoke(context, null, _pp);

            stackCalc.Push(returnvar);

            _codepos++;
        }
        //public void NewObj(ThreadContext context, Mono.Cecil.MethodReference method)
        //{
        //    object[] _pp = null;
        //    if (method.Parameters.Count > 0)
        //    {
        //        _pp = new object[method.Parameters.Count];
        //        for (int i = 0; i < _pp.Length; i++)
        //        {
        //            _pp[_pp.Length - 1 - i] = stackCalc.Pop();
        //        }
        //    }
        //    var typesys = context.environment.GetType(method.DeclaringType.FullName, method.Module);

        //    MethodParamList list = new MethodParamList(context.environment, method);

        //    object returnvar = typesys.GetMethod(method.Name, list).Invoke(context, null, _pp);

        //    stackCalc.Push(returnvar);




        //    _codepos++;

        //}
        public void Ldfld(ThreadContext context, IField field)
        {
            var obj = stackCalc.Pop();

            //var type = context.environment.GetType(field.DeclaringType.FullName, field.Module);
            //ar ff = type.GetField(field.Name);
            if (obj is RefObj)
            {
                obj = (obj as RefObj).Get();
            }
            var value = field.Get(obj);
            VBox box = ValueOnStack.MakeVBox(field.FieldType);
            if (box != null)
            {
                box.SetDirect(value);
                value = box;
            }
            stackCalc.Push(value);
            //System.Type t =obj.GetType();
            _codepos++;
        }
        public void Ldflda(ThreadContext context, IField field)
        {
            var obj = stackCalc.Pop();
            if (obj is RefObj)
            {
                obj = (obj as RefObj).Get();
            }
            // var type = context.environment.GetType(field.DeclaringType.FullName, field.Module);
            //var ff = type.GetField(field.Name);

            stackCalc.Push(new RefObj(field, obj));

            _codepos++;
        }
        public void Ldsfld(ThreadContext context, IField field)
        {
            //var type = context.environment.GetType(field.DeclaringType.FullName, field.Module);
            //var ff = type.GetField(field.Name);
            var value = field.Get(null);
            VBox box = ValueOnStack.MakeVBox(field.FieldType);
            if (box != null)
            {
                box.SetDirect(value);
                value = box;
            }
            stackCalc.Push(value);
            //System.Type t =obj.GetType();
            _codepos++;
        }
        public void Ldsflda(ThreadContext context, IField field)
        {
            //var type = context.environment.GetType(field.DeclaringType.FullName, field.Module);
            //var ff = type.GetField(field.Name);

            stackCalc.Push(new RefObj(field, null));

            _codepos++;
        }
        public void Stfld(ThreadContext context, IField field)
        {
            var value = stackCalc.Pop();

            var obj = stackCalc.Pop();
            //var type = context.environment.GetType(field.DeclaringType.FullName, field.Module);
            //var ff = type.GetField(field.Name);
            if (obj is RefObj)
            {
                var _this = (obj as RefObj).Get();
                if (_this == null && !field.isStatic)
                {
                    (obj as RefObj).Set(field.DeclaringType.InitObj());
                }
                obj = (obj as RefObj).Get();
            }
            if (value is VBox)
            {
                ValueOnStack.UnUse(value as VBox);
                value = (value as VBox).BoxDefine();
            }
            //else
            {//某些类型需要转换。。。
                VBox fbox = ValueOnStack.MakeVBox(field.FieldType);
                if (fbox != null)
                {
                    fbox.SetDirect(value);
                    value = fbox.BoxDefine();
                }
            }
            field.Set(obj, value);
            _codepos++;
        }
        public void Stsfld(ThreadContext context, IField field)
        {
            var value = stackCalc.Pop();
            //var obj = stackCalc.Pop();

            if (value is VBox)
            {
                ValueOnStack.UnUse(value as VBox);
                value = (value as VBox).BoxDefine();
            }
            //var type = context.environment.GetType(field.DeclaringType.FullName, field.Module);
            //var ff = type.GetField(field.Name);
            field.Set(null, value);

            _codepos++;
        }
        public void Constrained(ThreadContext context, ICLRType obj)
        {

            _codepos++;
        }
        public void Isinst(ThreadContext context, ICLRType _type)
        {
            var value = stackCalc.Pop();
            //var _type = context.environment.GetType(obj.FullName, obj.Module);
            if (_type.IsInst(value))
                stackCalc.Push(value);
            else
                stackCalc.Push(null);
            _codepos++;
        }
        public void Ldtoken(ThreadContext context, object token)
        {
            //string fname = obj.FullName;
            //string tfname = obj.FieldType.FullName;
            //var _type = context.environment.GetType(obj.DeclaringType.FullName, obj.Module);
            //var field = _type.GetField(obj.Name);

            stackCalc.Push(token);
            _codepos++;
        }

        public void Conv_Ovf_I1()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.SBYTE));
                ValueOnStack.UnUse(b);
            }
            else
            {
                stackCalc.Push((sbyte)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_U1()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.BYTE));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((byte)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_I2()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT16));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((Int16)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_U2()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT16));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((Int16)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_I4()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT32));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((Int32)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_U4()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT32));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((UInt32)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_I8()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT64));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((Int64)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_U8()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT64));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((Int64)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_I()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.INT32));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((Int32)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_U()
        {
            object num1 = stackCalc.Pop();
            VBox b = num1 as VBox;
            if (b != null)
            {
                stackCalc.Push(ValueOnStack.Convert(b, NumberType.UINT32));
                ValueOnStack.UnUse(b);

            }
            else
            {
                stackCalc.Push((UInt32)num1);
            }
            _codepos++;
        }
        public void Conv_Ovf_I1_Un()
        {
            throw new NotImplementedException();
        }

        public void Conv_Ovf_U1_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_I2_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_U2_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_I4_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_U4_Un()
        {
            throw new NotImplementedException();
        }

        public void Conv_Ovf_I8_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_U8_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_I_Un()
        {
            throw new NotImplementedException();
        }
        public void Conv_Ovf_U_Un()
        {
            throw new NotImplementedException();
        }

        public void Ldftn(ThreadContext context, IMethod method)
        {
            stackCalc.Push(new RefFunc(method, null));
            //throw new NotImplementedException();
            _codepos++;
        }
        public void Ldvirtftn(ThreadContext context, IMethod method)
        {
            object _this = stackCalc.Pop();
            stackCalc.Push(new RefFunc(method, _this));

            _codepos++;
        }

        public void Starg(ThreadContext context, int p)
        {
            object _this = stackCalc.Pop();
            if (_this is VBox)
            {
                ValueOnStack.UnUse(_this as VBox);

                _this = (_this as VBox).Clone();
            }
            this._params[p] = _this;
            _codepos++;
            //_codepos++;
        }
        //public void Ldarga(ThreadContext context, object obj)
        //{
        //    int pos=            (obj as Mono.Cecil.ParameterDefinition).Index;
        //    stackCalc.Push(new RefObj(this, pos, RefType.arg));
        //    Type t = obj.GetType();
        //    //throw new NotImplementedException(t.ToString());
        //    _codepos++;
        //}
        public void Calli(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }


        public void Break(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }

        public void Ldnull()
        {
            stackCalc.Push(null);
            _codepos++;
        }
        public void Jmp(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }

        public void Switch(ThreadContext context, int[] index)
        {
            var indexobj = stackCalc.Pop();
            uint pos = 0;
            if (indexobj is VBox)
            {
                pos = (indexobj as VBox).ToUInt();
                ValueOnStack.UnUse(indexobj as VBox);
            }
            else if (indexobj is int)
            {
                pos = (uint)(int)indexobj;
            }
            else
            {
                pos = uint.Parse(indexobj.ToString());
            }
            if (pos >= index.Length)
            {
                _codepos++;

            }
            else
            {
                _codepos = index[pos];
                //_pos = poss[pos];
            }
        }
        public void Ldind_I1()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_I1:");
            //_codepos++;
        }
        public void Ldind_U1()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_U1:");
            //_codepos++;
        }
        public void Ldind_I2()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_I2:");
            //_codepos++;
        }
        public void Ldind_U2()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_U2:");
            //_codepos++;
        }
        public void Ldind_I4()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_I4:");
            //_codepos++;
        }
        public void Ldind_U4()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_U4:");
            //_codepos++;
        }
        public void Ldind_I8()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_I8:");
            //_codepos++;
        }
        public void Ldind_I()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_I:");
            //_codepos++;
        }
        public void Ldind_R4()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_R4:");
            //_codepos++;
        }
        public void Ldind_R8()
        {
            object obje = stackCalc.Pop();
            if (obje is RefObj)
            {
                RefObj _ref = obje as RefObj;
                object value = _ref.Get();
                VBox box = ValueOnStack.MakeVBox(value.GetType());
                box.SetDirect(value);
                stackCalc.Push(box);
                _codepos++;
                return;
            }
            throw new Exception("not impl Ldind_R8:");
            //_codepos++;
        }
        public void Ldind_Ref()
        {

            throw new Exception("not impl Ldind_Ref:");
            //_codepos++;
        }
        public void Stind_Ref(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {

                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);
            }

            _codepos++;
        }
        public void Stind_I1(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {
                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);

            }

            _codepos++;
        }
        public void Stind_I2(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {
                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);

            }

            _codepos++;
        }
        public void Stind_I4(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {
                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);

            }

            _codepos++;
        }
        public void Stind_I8(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {
                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);

            }

            _codepos++;
        }
        public void Stind_R4(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {
                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);

            }

            _codepos++;
        }
        public void Stind_R8(ThreadContext context, object obj)
        {
            var o1 = stackCalc.Pop();
            var o2 = stackCalc.Pop();
            if (o2 is RefObj)
            {
                (o2 as RefObj).Set(o1 is VBox ? (o1 as VBox).BoxDefine() : o1);
                ValueOnStack.UnUse(o1 as VBox);

            }

            _codepos++;
        }
        public void And()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.And(n2);
            stackCalc.Push(n1);
            _codepos++;

            ValueOnStack.UnUse(n2);

        }
        public void Or()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Or(n2);
            stackCalc.Push(n1);
            _codepos++;

            ValueOnStack.UnUse(n2);

        }
        public void Xor()
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.Xor(n2);
            stackCalc.Push(n1);
            _codepos++;

            ValueOnStack.UnUse(n2);

        }
        public void Shl(ThreadContext context, object obj)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.v32 <<= n2.v32;
            stackCalc.Push(n1);
            _codepos++;

            ValueOnStack.UnUse(n2);
        }
        public void Shr(ThreadContext context, object obj)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.v32 >>= n2.v32;
            stackCalc.Push(n1);
            _codepos++;

            ValueOnStack.UnUse(n2);

        }
        public void Shr_Un(ThreadContext context, object obj)
        {
            VBox n2 = stackCalc.Pop() as VBox;
            VBox n1 = stackCalc.Pop() as VBox;
            n1.v32 >>= n2.v32;
            stackCalc.Push(n1);
            _codepos++;

            ValueOnStack.UnUse(n2);

        }
        public void Not()
        {

            VBox n1 = stackCalc.Pop() as VBox;
            n1.Not();
            stackCalc.Push(n1);
            _codepos++;


        }
        public void Cpobj(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Ldobj(ThreadContext context, object obj)
        {
            var pos = stackCalc.Pop() as RefObj;

            stackCalc.Push(pos.Get());

            _codepos++;
        }
        public void Castclass(ThreadContext context, ICLRType _type)
        {
            if (_type is ICLRType_System)
            {
                var obj = stackCalc.Peek();
                if (obj != null)
                {
                    var ssypt = (_type as ICLRType_System).TypeForSystem;
                    var otype = obj.GetType();

                    if (otype != ssypt && obj.GetType().IsSubclassOf(ssypt) == false)
                    {
                        throw new Exception("不可转换");
                    }


                }
            }


            //throw new NotImplementedException(t.ToString());
            _codepos++;
        }
        public void Throw(ThreadContext context, object obj)
        {
            Exception exc = stackCalc.Pop() as Exception;
            throw exc;
            //_codepos++;
        }
        public void Stobj(ThreadContext context, object obj)
        {
            var v = stackCalc.Pop();
            var addr = stackCalc.Pop() as RefObj;
            addr.Set(v);
            _codepos++;
        }
        public void Refanyval(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Mkrefany(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }

        public void Add_Ovf(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Add_Ovf_Un(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Mul_Ovf(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Mul_Ovf_Un(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Sub_Ovf(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Sub_Ovf_Un(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Endfinally(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Stind_I(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Arglist(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }

        public void Localloc(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Endfilter(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Unaligned(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Volatile()
        {
            _codepos++;
        }
        //ThreadContext context, object obj)
        //{
        //    Type t = obj.GetType();
        //    throw new NotImplementedException(t.ToString());
        //    //_codepos++;
        //}
        public void Tail(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Initobj(ThreadContext context, ICLRType _type)
        {
            RefObj _this = stackCalc.Pop() as RefObj;

            //var typesys = context.environment.GetType(method.DeclaringType.FullName, method.Module);
            var _object = _type.InitObj();

            _this.Set(_object);

            _codepos++;
        }
        public void Cpblk(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Initblk(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void No(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Rethrow(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Sizeof(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Refanytype(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
        public void Readonly(ThreadContext context, object obj)
        {
            Type t = obj.GetType();
            throw new NotImplementedException(t.ToString());
            //_codepos++;
        }
    }
}
