using dnlib.DotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HybridCLR.Editor.Meta
{
    public class GenericClass
    {
        public TypeDef Type { get; }

        public List<TypeSig> KlassInst { get; }

        private readonly int _hashCode;

        public GenericClass(TypeDef type, List<TypeSig> classInst)
        {
            Type = type;
            KlassInst = classInst;
            _hashCode = ComputHashCode();
        }

        public GenericClass ToGenericShare()
        {
            return new GenericClass(Type, MetaUtil.ToShareTypeSigs(Type.Module.CorLibTypes, KlassInst));
        }

        public override bool Equals(object obj)
        {
            if (obj is GenericClass gc)
            {
                return Type == gc.Type && MetaUtil.EqualsTypeSigArray(KlassInst, gc.KlassInst);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int ComputHashCode()
        {
            int hash = TypeEqualityComparer.Instance.GetHashCode(Type);
            if (KlassInst != null)
            {
                hash = HashUtil.CombineHash(hash, HashUtil.ComputHash(KlassInst));
            }
            return hash;
        }

        public TypeSig ToTypeSig()
        {
            return new GenericInstSig(this.Type.ToTypeSig().ToClassOrValueTypeSig(), this.KlassInst);
        }

        public static GenericClass ResolveClass(TypeSpec type, GenericArgumentContext ctx)
        {
            var sig = type.TypeSig.ToGenericInstSig();
            if (sig == null)
            {
                return null;
            }
            TypeDef def = type.ResolveTypeDef();
            if (def == null)
            {
                Debug.LogWarning($"type:{type} ResolveTypeDef() == null");
                return null;
            }
            var klassInst = ctx != null ? sig.GenericArguments.Select(ga => MetaUtil.Inflate(ga, ctx)).ToList() : sig.GenericArguments.ToList();
            return new GenericClass(def, klassInst);
        }
    }
}
