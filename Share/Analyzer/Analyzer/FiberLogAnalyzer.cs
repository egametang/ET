using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FiberLogAnalyzer:DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FiberLogAnalyzerRule.Rule);
    
    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction((analysisContext =>
        {
            if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.AllModelHotfix))
            {
                analysisContext.RegisterSyntaxNodeAction(AnalyzeMemberAccessExpression,SyntaxKind.InvocationExpression);
            }
        } ));
    }

    private void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (!(context.Node is InvocationExpressionSyntax invocationExpressionSyntax))
        {
            return;
        }

        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;

        if (methodSymbol==null)
        {
            return;
        }

        // 筛选出调用ET.Log的日志输出
        if (methodSymbol.ContainingType.ToString() != Definition.ETLog)
        {
            return;
        }

        if (invocationExpressionSyntax.GetNeareastAncestor<ClassDeclarationSyntax>() is not ClassDeclarationSyntax parentClassSyntax)
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(parentClassSyntax) is not INamedTypeSymbol parentClassSymbol)
        {
            return;
        }

        // 判断是否在实体类内部调用
        if (parentClassSymbol.ToString()==Definition.EntityType|| parentClassSymbol.ToString()==Definition.LSEntityType || parentClassSymbol.BaseType?.ToString()==Definition.EntityType ||parentClassSymbol.BaseType?.ToString()==Definition.LSEntityType)
        {
            Diagnostic diagnostic = Diagnostic.Create(FiberLogAnalyzerRule.Rule, invocationExpressionSyntax?.GetLocation());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // 判断是否在含实体类参数的函数内调用
        if (invocationExpressionSyntax.GetNeareastAncestor<MethodDeclarationSyntax>() is not MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is not IMethodSymbol containningMethodSymbol)
        {
            return;
        }

        foreach (var parameter in containningMethodSymbol.Parameters)
        {
            var parameterType = parameter.Type.ToString();
            var parameterBaseType = parameter.Type.BaseType?.ToString();
            if (parameterType== Definition.EntityType|| parameterType==Definition.LSEntityType ||parameterBaseType==Definition.EntityType ||parameterBaseType==Definition.LSEntityType)
            {
                Diagnostic diagnostic = Diagnostic.Create(FiberLogAnalyzerRule.Rule, invocationExpressionSyntax?.GetLocation());
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
    }
}