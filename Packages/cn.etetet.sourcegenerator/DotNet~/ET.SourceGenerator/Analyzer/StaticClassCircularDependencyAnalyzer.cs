using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticClassCircularDependencyAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "静态类之间禁止环形依赖";

        private const string MessageFormat = "ET0013 静态类函数引用存在环形依赖 请修改为单向依赖 {0}";

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
            var dependencyMap = new ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>>();
            var staticClassSet = new HashSet<string>();
            
            if (AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                
                context.RegisterSyntaxNodeAction(analysisContext => { this.StaticClassDependencyAnalyze(analysisContext, dependencyMap, staticClassSet); }, SyntaxKind.InvocationExpression);
                context.RegisterCompilationEndAction(analysisContext => { this.CircularDependencyAnalyze(analysisContext, dependencyMap, staticClassSet); });
            }
        }

        
        /// <summary>
        /// 静态类依赖分析 构建depedencyMap
        /// </summary>
        private void StaticClassDependencyAnalyze(SyntaxNodeAnalysisContext context, ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>> dependencyMap,
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
                dependencyMap[methodClassTypeName] = new ();
            }

            var dic = dependencyMap[methodClassTypeName];
            lock (dic)
            {
                List<InvocationExpressionSyntax> invocationExpressionSyntaxes;
                if (!dic.TryGetValue(selfClassTypeName,out invocationExpressionSyntaxes))
                {
                    invocationExpressionSyntaxes = new List<InvocationExpressionSyntax>();
                    dic.Add(selfClassTypeName,invocationExpressionSyntaxes);
                }
                invocationExpressionSyntaxes.Add(invocationExpressionSyntax);
            }
        }

        /// <summary>
        /// 环形依赖分析
        /// </summary>
        private void CircularDependencyAnalyze(CompilationAnalysisContext context, ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>> dependencyMap,
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
                        if (dependency.Value.ContainsKey(noDependencyStaticClass))
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
            
            var staticClassDependencyMap = new ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>>();
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
                // 找出所有的环
                var visited = new HashSet<string>();
                var stack = new Stack<string>();
                var path = new List<string>();
                foreach (var staticClass in staticClassSet)
                {
                    this.FindAllCycles(staticClass, staticClassDependencyMap, visited, stack, path, context);
                }
            }
        }
        
        /// <summary>
        /// 获取没有被任何其他静态类引用的静态类
        /// </summary>
        private string GetClassNotReferencedByOtherStaticClass(ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>> dependencyMap,
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
        private string GetClassNotReferenceAnyStaticClass(ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>> dependencyMap,
        HashSet<string> staticClassSet)
        {
            foreach (string? staticClass in staticClassSet)
            {
                var result = dependencyMap.Where(x => x.Value.ContainsKey(staticClass));
                if (!result.Any())
                {
                    return staticClass;
                }
            }

            return string.Empty;
        }
        
        private void FindAllCycles(string currentClass, ConcurrentDictionary<string, Dictionary<string, List<InvocationExpressionSyntax>>> dependencyMap,
        HashSet<string> visited, Stack<string> stack, List<string> path, CompilationAnalysisContext context)
        {
            if (stack.Contains(currentClass))
            {
                int index = path.IndexOf(currentClass);
                var cyclePath = path.Skip(index).ToList();
                cyclePath.Add(currentClass);
                cyclePath.Reverse();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < cyclePath.Count-1; i++)
                {
                    var invokeClass = cyclePath[i];
                    var beInvokedClass = cyclePath[i + 1];

                    sb.Append($" {invokeClass} -> {beInvokedClass} invocation:( ");
                    foreach (var invocation in dependencyMap[beInvokedClass][invokeClass])
                    {
                        sb.Append(invocation.ToString());
                        sb.Append(" ");
                    }

                    sb.Append(")");
                }
                
                var diagnostic = Diagnostic.Create(Rule, Location.None, sb);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            if (visited.Contains(currentClass))
            {
                return;
            }

            visited.Add(currentClass);
            stack.Push(currentClass);
            path.Add(currentClass);

            if (dependencyMap.ContainsKey(currentClass))
            {
                foreach (var dependency in dependencyMap[currentClass].Keys)
                {
                    this.FindAllCycles(dependency, dependencyMap, visited, stack, path, context);
                }
            }

            stack.Pop();
            path.RemoveAt(path.Count - 1);
        }
        
    }
}