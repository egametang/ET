using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger
{
    public enum VariableTypes
    {
        Normal,
        FieldReference,
        PropertyReference,
        TypeReference,
        IndexAccess,
        Invocation,
        Integer,
        Boolean,
        String,
        Null,
        Error,
        NotFound,
        Timeout,
        Pending,
    }

    public enum ValueTypes
    {
        Object,
        Null,
        Integer,
        Boolean,
        String,
    }

    public class VariableReference
    {
        public long Address { get; set; }
        public VariableTypes Type { get; set; }
        public int Offset { get; set; }
        public string Name { get; set; }
        public VariableReference Parent { get; set; }
        public VariableReference[] Parameters { get; set; }

        public string FullName
        {
            get
            {
                if (Parent != null)
                {
                    switch (Type)
                    {
                        case VariableTypes.FieldReference:
                        case VariableTypes.PropertyReference:
                            return string.Format("{0}.{1}", Parent.FullName, Name);
                        case VariableTypes.IndexAccess:
                            return string.Format("{0}[{1}]", Parent.FullName, Parameters[0].FullName);
                        case VariableTypes.Error:
                            return Name;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    switch (Type)
                    {
                        case VariableTypes.String:
                            return string.Format("\"{0}\"", Name);
                        case VariableTypes.Integer:
                            return Offset.ToString();
                        case VariableTypes.Boolean:
                            return (Offset == 1).ToString();
                        default:
                            return Name;
                    }
                }
            }
        }

        public static VariableReference Null = new VariableReference
        {
            Type = VariableTypes.Null,
            Name = "null"
        };

        public static VariableReference True = new VariableReference
        {
            Type = VariableTypes.Boolean,
            Name = "true",
            Offset = 1,
        };

        public static VariableReference False = new VariableReference
        {
            Type = VariableTypes.Boolean,
            Name = "false",
            Offset = 0
        };

        public static VariableReference GetInteger(int val)
        {
            var res = new VariableReference();
            res.Type = VariableTypes.Integer;
            res.Name = "";
            res.Offset = val;
            
            return res;
        }

        public static VariableReference GetString(string val)
        {
            var res = new VariableReference();
            res.Type = VariableTypes.String;
            res.Name = val;

            return res;
        }

        public static VariableReference GetMember(string name, VariableReference parent)
        {
            var res = new VariableReference();
            res.Type = VariableTypes.FieldReference;
            res.Name = name;
            res.Parent = parent;

            return res;
        }
    }

    public class VariableInfo
    {
        public long Address { get; set; }
        public VariableTypes Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public ValueTypes ValueType { get; set; }
        public bool Expandable { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsProtected { get; set; }
        public int Offset { get; set;}

        public static VariableInfo FromObject(object obj, bool retriveType = false)
        {
            VariableInfo info = new VariableInfo();
            info.Name = "";
            if (obj != null)
            {
                info.Value = obj.ToString();
                if(obj is int)
                {
                    info.ValueType = ValueTypes.Integer;
                }
                else if(obj is bool)
                {
                    info.ValueType = ValueTypes.Boolean;
                }
                else if(obj is string)
                {
                    info.ValueType = ValueTypes.String;
                }
                if (retriveType)
                {
                    if (obj is Runtime.Intepreter.ILTypeInstance)
                    {
                        info.TypeName = ((Intepreter.ILTypeInstance)obj).Type.FullName;                        
                    }
                    else if (obj is Enviorment.CrossBindingAdaptorType)
                    {
                        info.TypeName = ((Enviorment.CrossBindingAdaptorType)obj).ILInstance.Type.FullName;
                    }
                    else
                    {
                        info.TypeName = obj.GetType().FullName;
                    }
                }
                info.Expandable = !obj.GetType().IsPrimitive && !(obj is string);
            }
            else
            {
                info.Value = "null";
                info.ValueType = ValueTypes.Null;
            }

            return info;
        }

        public static VariableInfo NullReferenceExeption = new VariableInfo
        {
            Type = VariableTypes.Error,
            Name = "",
            TypeName = "",
            Value = "NullReferenceException"
        };

        public static VariableInfo RequestTimeout = new VariableInfo
        {
            Type = VariableTypes.Timeout,
            Name = "",
            TypeName = "",
            Value = "RequestTimeoutException"
        };

        public static VariableInfo Null = new VariableInfo
        {
            Type = VariableTypes.Null,
            Name = "",
            TypeName = "",
            Value = "null",
            ValueType = ValueTypes.Null
        };

        public static VariableInfo True = new VariableInfo
        {
            Type = VariableTypes.Boolean,
            Name = "",
            TypeName = "System.Boolean",
            Value = "true",
            ValueType = ValueTypes.Boolean
        };

        public static VariableInfo False = new VariableInfo
        {
            Type = VariableTypes.Boolean,
            Name = "",
            TypeName = "System.Boolean",
            Value = "false",
            ValueType = ValueTypes.Boolean
        };

        public static VariableInfo GetCannotFind(string name)
        {
            var res = new VariableInfo
            {
                Type = VariableTypes.NotFound,
                TypeName = "",
            };
            res.Name = name;
            res.Value = string.Format("Cannot find {0} in current scope.", name);

            return res;
        }

        public static VariableInfo GetInteger(int val)
        {
            var res = new VariableInfo();
            res.Type = VariableTypes.Integer;
            res.Value = val.ToString();
            res.TypeName = "System.Int32";
            res.Name = "";
            res.ValueType = ValueTypes.Integer;

            return res;
        }

        public static VariableInfo GetString(string val)
        {
            var res = new VariableInfo();
            res.Type = VariableTypes.String;
            res.Value = val;
            res.TypeName = "System.String";
            res.Name = "";
            res.ValueType = ValueTypes.String;

            return res;
        }

        public static VariableInfo GetException(Exception ex)
        {
            var res = new VariableInfo();
            res.Type = VariableTypes.Error;
            res.Value = ex.ToString();
            res.TypeName = ex.GetType().FullName;
            res.Name = "";
            res.ValueType = ValueTypes.String;

            return res;
        }
    }
}
