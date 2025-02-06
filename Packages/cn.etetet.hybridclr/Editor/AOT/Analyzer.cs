using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.AOT
{

    public class Analyzer
    {
        public class Options
        {
            public AssemblyReferenceDeepCollector Collector { get; set; }

            public int MaxIterationCount { get; set; }

            public bool ComputeAotAssembly { get; set; }
        }

        private readonly int _maxInterationCount;

        private readonly AssemblyReferenceDeepCollector _assemblyCollector;

        private readonly bool _computeAotAssembly;

        private readonly HashSet<GenericClass> _genericTypes = new HashSet<GenericClass>();
        private readonly HashSet<GenericMethod> _genericMethods = new HashSet<GenericMethod>();

        private List<GenericMethod> _processingMethods = new List<GenericMethod>();
        private List<GenericMethod> _newMethods = new List<GenericMethod>();

        public IReadOnlyCollection<GenericClass> GenericTypes => _genericTypes;

        public IReadOnlyCollection<GenericMethod> GenericMethods => _genericMethods;

        private readonly MethodReferenceAnalyzer _methodReferenceAnalyzer;

        private readonly HashSet<string> _hotUpdateAssemblyFiles;

        public ConstraintContext ConstraintContext { get; } = new ConstraintContext();

        public List<GenericClass> AotGenericTypes { get; } = new List<GenericClass>();

        public List<GenericMethod> AotGenericMethods { get; } = new List<GenericMethod>();

        public Analyzer(Options options)
        {
            _assemblyCollector = options.Collector;
            _maxInterationCount = options.MaxIterationCount;
            _computeAotAssembly = options.ComputeAotAssembly;
            _methodReferenceAnalyzer = new MethodReferenceAnalyzer(this.OnNewMethod);
            _hotUpdateAssemblyFiles = new HashSet<string>(options.Collector.GetRootAssemblyNames().Select(assName => assName + ".dll"));
        }

        private void TryAddAndWalkGenericType(GenericClass gc)
        {
            if (gc == null)
            {
                return;
            }
            gc = gc.ToGenericShare();
            if (_genericTypes.Add(gc) && NeedWalk(null, gc.Type))
            {
                WalkType(gc);
            }
        }

        private bool NeedWalk(MethodDef callFrom, TypeDef type)
        {
            return _hotUpdateAssemblyFiles.Contains(type.Module.Name) || callFrom == null || callFrom.HasGenericParameters;
        }

        private bool IsAotType(TypeDef type)
        {
            return _computeAotAssembly || !_hotUpdateAssemblyFiles.Contains(type.Module.Name);
        }

        private bool IsAotGenericMethod(MethodDef method)
        {
            return IsAotType(method.DeclaringType) && method.HasGenericParameters;
        }

        private void OnNewMethod(MethodDef methodDef, List<TypeSig> klassGenericInst, List<TypeSig> methodGenericInst, GenericMethod method)
        {
            if(method == null)
            {
                return;
            }
            if (NeedWalk(methodDef, method.Method.DeclaringType) && _genericMethods.Add(method))
            {
                _newMethods.Add(method);
            }
            if (method.KlassInst != null)
            {
                TryAddAndWalkGenericType(new GenericClass(method.Method.DeclaringType, method.KlassInst));
            }
        }

        private void TryAddMethodNotWalkType(GenericMethod method)
        {
            if (method == null)
            {
                return;
            }
            if (NeedWalk(null, method.Method.DeclaringType) && _genericMethods.Add(method))
            {
                _newMethods.Add(method);
            }
        }

        private void WalkType(GenericClass gc)
        {
            //Debug.Log($"typespec:{sig} {sig.GenericType} {sig.GenericType.TypeDefOrRef.ResolveTypeDef()}");
            //Debug.Log($"== walk generic type:{new GenericInstSig(gc.Type.ToTypeSig().ToClassOrValueTypeSig(), gc.KlassInst)}");
            ITypeDefOrRef baseType = gc.Type.BaseType;
            if (baseType != null && baseType.TryGetGenericInstSig() != null)
            {
                GenericClass parentType = GenericClass.ResolveClass((TypeSpec)baseType, new GenericArgumentContext(gc.KlassInst, null));
                TryAddAndWalkGenericType(parentType);
            }
            foreach (var method in gc.Type.Methods)
            {
                if (method.HasGenericParameters || !method.HasBody || method.Body.Instructions == null)
                {
                    continue;
                }
                var gm = new GenericMethod(method, gc.KlassInst, null).ToGenericShare();
                //Debug.Log($"add method:{gm.Method} {gm.KlassInst}");
                TryAddMethodNotWalkType(gm);
            }
        }

        private void WalkType(TypeDef typeDef)
        {
            if (typeDef.HasGenericParameters)
            {
                return;
            }
            ITypeDefOrRef baseType = typeDef.BaseType;
            if (baseType != null && baseType.TryGetGenericInstSig() != null)
            {
                GenericClass gc = GenericClass.ResolveClass((TypeSpec)baseType, null);
                TryAddAndWalkGenericType(gc);
            }
        }

        private void Prepare()
        {
            // 将所有非泛型函数全部加入函数列表，同时立马walk这些method。
            // 后续迭代中将只遍历MethodSpec
            foreach (var ass in _assemblyCollector.GetLoadedModulesOfRootAssemblies())
            {
                foreach (TypeDef typeDef in ass.GetTypes())
                {
                    WalkType(typeDef);
                }

                for (uint rid = 1, n = ass.Metadata.TablesStream.TypeSpecTable.Rows; rid <= n; rid++)
                {
                    var ts = ass.ResolveTypeSpec(rid);
                    var cs = GenericClass.ResolveClass(ts, null)?.ToGenericShare();
                    if (cs != null)
                    {
                        TryAddAndWalkGenericType(cs);
                    }
                }

                for (uint rid = 1, n = ass.Metadata.TablesStream.MethodSpecTable.Rows; rid <= n; rid++)
                {
                    var ms = ass.ResolveMethodSpec(rid);
                    var gm = GenericMethod.ResolveMethod(ms, null)?.ToGenericShare();
                    TryAddMethodNotWalkType(gm);
                }
            }
            Debug.Log($"PostPrepare genericTypes:{_genericTypes.Count} genericMethods:{_genericMethods.Count} newMethods:{_newMethods.Count}");
        }

        private void RecursiveCollect()
        {
            for (int i = 0; i < _maxInterationCount && _newMethods.Count > 0; i++)
            {
                var temp = _processingMethods;
                _processingMethods = _newMethods;
                _newMethods = temp;
                _newMethods.Clear();

                foreach (var method in _processingMethods)
                {
                    _methodReferenceAnalyzer.WalkMethod(method.Method, method.KlassInst, method.MethodInst);
                }
                Debug.Log($"iteration:[{i}] genericClass:{_genericTypes.Count} genericMethods:{_genericMethods.Count} newMethods:{_newMethods.Count}");
            }
        }

        private bool IsNotShareableAOTGenericType(TypeDef typeDef)
        {
            if (!IsAotType(typeDef))
            {
                return false;
            }
            return typeDef.GenericParameters.Any(c => !c.HasReferenceTypeConstraint);
        }

        private bool IsNotShareableAOTGenericMethod(MethodDef method)
        {
            if (!IsAotGenericMethod(method))
            {
                return false;
            }
            return method.GenericParameters.Concat(method.DeclaringType.GenericParameters).Any(c => !c.HasReferenceTypeConstraint);
        }

        private void FilterAOTGenericTypeAndMethods()
        {
            ConstraintContext cc = this.ConstraintContext;
            AotGenericTypes.AddRange(_genericTypes.Where(type => IsNotShareableAOTGenericType(type.Type)).Select(gc => cc.ApplyConstraints(gc)));
            AotGenericMethods.AddRange(_genericMethods.Where(method => IsNotShareableAOTGenericMethod(method.Method)).Select(gm => cc.ApplyConstraints(gm)));
        }

        public void Run()
        {
            Prepare();
            RecursiveCollect();
            FilterAOTGenericTypeAndMethods();
        }
    }
}
