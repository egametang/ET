using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HybridCLR.Editor.Meta
{
    public class GenericMethod
    {
        public MethodDef Method { get; }

        public List<TypeSig> KlassInst { get; }

        public List<TypeSig> MethodInst { get; }

        private readonly int _hashCode;

        public GenericMethod(MethodDef method, List<TypeSig> classInst, List<TypeSig> methodInst)
        {
            Method = method;
            KlassInst = classInst;
            MethodInst = methodInst;
            _hashCode = ComputHashCode();
        }

        public GenericMethod ToGenericShare()
        {
            ICorLibTypes corLibTypes = Method.Module.CorLibTypes;
            return new GenericMethod(Method, MetaUtil.ToShareTypeSigs(corLibTypes, KlassInst), MetaUtil.ToShareTypeSigs(corLibTypes, MethodInst));
        }

        public override bool Equals(object obj)
        {
            GenericMethod o = (GenericMethod)obj;
            return Method == o.Method
                && MetaUtil.EqualsTypeSigArray(KlassInst, o.KlassInst)
                && MetaUtil.EqualsTypeSigArray(MethodInst, o.MethodInst);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return $"{Method}|{string.Join(",", (IEnumerable<TypeSig>)KlassInst ?? Array.Empty<TypeSig>())}|{string.Join(",", (IEnumerable<TypeSig>)MethodInst ?? Array.Empty<TypeSig>())}";
        }

        private int ComputHashCode()
        {
            int hash = MethodEqualityComparer.CompareDeclaringTypes.GetHashCode(Method);
            if (KlassInst != null)
            {
                hash = HashUtil.CombineHash(hash, HashUtil.ComputHash(KlassInst));
            }
            if (MethodInst != null)
            {
                hash = HashUtil.CombineHash(hash, HashUtil.ComputHash(MethodInst));
            }
            return hash;
        }

        public MethodSpec ToMethodSpec()
        {
            IMethodDefOrRef mt = KlassInst != null ? 
                (IMethodDefOrRef)new MemberRefUser(this.Method.Module, Method.Name, Method.MethodSig, new TypeSpecUser(new GenericInstSig(this.Method.DeclaringType.ToTypeSig().ToClassOrValueTypeSig(), this.KlassInst)))
                : this.Method;
            return new MethodSpecUser(mt, new GenericInstMethodSig(MethodInst));
        }

        public static GenericMethod ResolveMethod(IMethod method, GenericArgumentContext ctx)
        {
            //Debug.Log($"== resolve method:{method}");
            TypeDef typeDef = null;
            List<TypeSig> klassInst = null;
            List<TypeSig> methodInst = null;

            MethodDef methodDef = null;


            var decalringType = method.DeclaringType;
            typeDef = decalringType.ResolveTypeDef();
            if (typeDef == null)
            {
                return null;
            }
            GenericInstSig gis = decalringType.TryGetGenericInstSig();
            if (gis != null)
            {
                klassInst = ctx != null ? gis.GenericArguments.Select(ga => MetaUtil.Inflate(ga, ctx)).ToList() : gis.GenericArguments.ToList();
            }
            methodDef = method.ResolveMethodDef();
            if (methodDef == null)
            {
                Debug.LogWarning($"method:{method} ResolveMethodDef() == null");
                return null;
            }
            if (method is MethodSpec methodSpec)
            {
                methodInst = ctx != null ? methodSpec.GenericInstMethodSig.GenericArguments.Select(ga => MetaUtil.Inflate(ga, ctx)).ToList()
                    : methodSpec.GenericInstMethodSig.GenericArguments.ToList();
            }
            return new GenericMethod(methodDef, klassInst, methodInst);
        }

    }
}
