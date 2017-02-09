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
        Error,
    }

    public class VariableReference
    {
        public long Address { get; set; }
        public VariableTypes Type { get; set; }
        public int Offset { get; set; }
        public VariableReference Parent { get; set; }
    }

    public class VariableInfo
    {
        public long Address { get; set; }
        public VariableTypes Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public bool Expandable { get; set; }
        public int Offset { get; set;}
    }
}
