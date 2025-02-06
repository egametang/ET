using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.AOT
{

    public class ConstraintContext
    {
        public class ImplType
        {
            public TypeSig BaseType { get; }

            public List<TypeSig> Interfaces { get; }

            public bool ValueType { get; }

            private readonly int _hash;

            public ImplType(TypeSig baseType, List<TypeSig> interfaces, bool valueType)
            {
                BaseType = baseType;
                Interfaces = interfaces;
                ValueType = valueType;
                _hash = ComputHash();
            }

            public override bool Equals(object obj)
            {
                ImplType o = (ImplType)obj;
                return MetaUtil.EqualsTypeSig(this.BaseType, o.BaseType)
                    && MetaUtil.EqualsTypeSigArray(this.Interfaces, o.Interfaces)
                    && this.ValueType == o.ValueType;
            }

            public override int GetHashCode()
            {
                return _hash;
            }

            private int ComputHash()
            { 
                int hash = 0;
                if (BaseType != null)
                {
                    hash = HashUtil.CombineHash(hash, TypeEqualityComparer.Instance.GetHashCode(BaseType));
                }
                if (Interfaces.Count > 0)
                {
                    hash = HashUtil.CombineHash(hash, HashUtil.ComputHash(Interfaces));
                }

                return hash;
            }
        }

        public HashSet<ImplType> ImplTypes { get; } = new HashSet<ImplType>();

        public GenericClass ApplyConstraints(GenericClass gc)
        {
            return gc;
        }

        public GenericMethod ApplyConstraints(GenericMethod gm)
        {
            return gm;
        }
    }
}
