using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticClassCircularDependencyAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "静态类之间禁止环形依赖";

        private const string MessageFormat = "ET0013 静态类函数引用存在环形依赖 请修改为单向依赖 静态类{0}被 静态类{1}引用";

        private const string Description = "静态类之间禁止环形依赖.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.StaticClassCircularDedendencyAnalyzerRuleId,
            Title,
            MessageFormat,
            DiagnosticCategories.Hotfix,
            DiagnosticSeverity.Error, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        
        private object lockObj = new object();

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(this.CompilationStartAnalysis);
        }

        private void CompilationStartAnalysis(CompilationStartAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                return;
            }

            if (context.Compilation.AssemblyName == null)
            {
                return;
            }

            var dependencyMap = new ConcurrentDictionary<string, HashSet<string>>();
            var staticClassSet = new HashSet<string>();

            context.RegisterSyntaxNodeAction(
                analysisContext => { this.StaticClassDependencyAnalyze(analysisContext, dependencyMap, staticClassSet); },
                SyntaxKind.InvocationExpression);

            context.RegisterCompilationEndAction(analysisContext => { this.CircularDependencyAnalyze(analysisContext, dependencyMap, staticClassSet); });
        }

        

        /// <summary>
        /// 静态类依赖分析 构建depedencyMap
        /// </summary>
        private void StaticClassDependencyAnalyze(SyntaxNodeAnalysisContext context, ConcurrentDictionary<string, HashSet<string>> dependencyMap,
        HashSet<string> staticClassSet)
        {
            if (!(context.Node is InvocationExpressionSyntax invocationExpressionSyntax))
            {
                return;
            }

            if (context.ContainingSymbol == null)
            {
                return;
            }

            INamedTypeSymbol? selfClassType = context.ContainingSymbol.ContainingType;

            //筛选出自身为静态类
            if (selfClassType == null || !selfClassType.IsStatic)
            {
                return;
            }

            //筛选出函数调用
            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            string selfClassTypeName = selfClassType.ToString();

            string methodClassTypeName = methodSymbol.ContainingType.ToString();

            lock (this.lockObj)
            {
                if (!staticClassSet.Contains(selfClassTypeName))
                {
                    staticClassSet.Add(selfClassTypeName);
                }
            }

            // 筛选出对其他静态类的函数调用
            if (selfClassTypeName == methodClassTypeName)
            {
                return;
            }

            if (!methodSymbol.ContainingType.IsStatic)
            {
                return;
            }

            if (!dependencyMap.ContainsKey(methodClassTypeName))
            {
                dependencyMap[methodClassTypeName] = new HashSet<string>();
            }

            var set = dependencyMap[methodClassTypeName];
            lock (set)
            {
                if (!set.Contains(selfClassTypeName))
                {
                    set.Add(selfClassTypeName);
                }
            }
            
        }

        /// <summary>
        /// 环形依赖分析
        /// </summary>
        private void CircularDependencyAnalyze(CompilationAnalysisContext context, ConcurrentDictionary<string, HashSet<string>> dependencyMap,
        HashSet<string> staticClassSet)
        {
            
            // 排除只引用其他静态类的静态类
            while (true)
            {
                string noDependencyStaticClass = this.GetClassNotReferencedByOtherStaticClass(dependencyMap, staticClassSet);
                if (!string.IsNullOrEmpty(noDependencyStaticClass))
                {
                    foreach (var dependency in dependencyMap)
                    {
                        if (dependency.Value.Contains(noDependencyStaticClass))
                        {
                            dependency.Value.Remove(noDependencyStaticClass);
                        }
                    }

                    staticClassSet.Remove(noDependencyStaticClass);
                }
                else
                {
                    break;
                }
            }
            
            var staticClassDependencyMap = new ConcurrentDictionary<string, HashSet<string>>();
            foreach (string? staticClass in staticClassSet)
            {
                staticClassDependencyMap[staticClass] = dependencyMap[staticClass];
            }
            
            //排除只被其他静态类引用的静态类
            while (true)
            {
                string staticClass = this.GetClassNotReferenceAnyStaticClass(staticClassDependencyMap, staticClassSet);
                if (!string.IsNullOrEmpty(staticClass))
                {
                    staticClassSet.Remove(staticClass);
                }
                else
                {
                    break;
                }
            }

            if (staticClassSet.Count > 0)
            {
                foreach (string? staticClass in staticClassSet)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, null, staticClass,
                        FormatSet(dependencyMap[staticClass]));
                    context.ReportDiagnostic(diagnostic);
                }
            }

            string FormatSet(HashSet<string> hashSet)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string? value in hashSet)
                {
                    stringBuilder.Append($"{value} ");
                }
                return stringBuilder.ToString();
            }
        }
        
        /// <summary>
        /// 获取没有被任何其他静态类引用的静态类
        /// </summary>
        private string GetClassNotReferencedByOtherStaticClass(ConcurrentDictionary<string, HashSet<string>> dependencyMap,
        HashSet<string> staticClassSet)
        {
            foreach (string? staticClass in staticClassSet)
            {
                // 该静态类  没有被任何其他静态类引用
                if (!dependencyMap.ContainsKey(staticClass) || dependencyMap[staticClass].Count == 0)
                {
                    return staticClass;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取没有引用任何其他静态类的静态类
        /// </summary>
        private string GetClassNotReferenceAnyStaticClass(ConcurrentDictionary<string, HashSet<string>> dependencyMap,
        HashSet<string> staticClassSet)
        {
            foreach (string? staticClass in staticClassSet)
            {
                var result = dependencyMap.Where(x => x.Value.Contains(staticClass));
                if (result.Count() == 0)
                {
                    return staticClass;
                }
            }

            return string.Empty;
        }
    }
}